using ACadSharp.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

/// <summary>Writes modeler payloads and their handle search index (DWG chapter 24).</summary>
internal static class DwgPrototype1bWriter
{
	internal static byte[] Write(IEnumerable<ModelerGeometry> geometry)
	{
		var entities = geometry.OrderBy(e => e.Handle).ToArray();
		// A page and its segment/page headers fit within one MiB.
		const int pageSize = 0xfffb0;
		int segmentCount = 6;
		var records = entities.Select(entity =>
		{
			int count = entity.AcisData.Length > 0x40000 ? (entity.AcisData.Length - 1) / pageSize + 1 : 0;
			var pages = Enumerable.Range(0, count).Select(i => (Segment: segmentCount++, Offset: i * pageSize,
				Size: Math.Min(pageSize, entity.AcisData.Length - i * pageSize))).ToArray();
			return (Entity: entity, Pages: pages, Size: count == 0 ? 4 + entity.AcisData.Length : 36 + count * 8);
		}).ToArray();
		var segments = new List<byte[]>();
		void segment(string name, StorageBytes body, int systemOffset = 0, int objectOffset = 0, bool blob = false)
		{
			using var header = new StorageBytes();
			header.U16(0xd5ac);
			header.Write(Encoding.ASCII.GetBytes(name));
			header.U32(segments.Count);
			header.U32(blob ? 1 : 0);
			header.U32((48 + body.Length + 63) / 64 * 64);
			header.U32(0); header.U32(1); header.U32(0);
			header.U32(systemOffset); header.U32(objectOffset);
			header.Write(Enumerable.Repeat((byte)0x55, 8).ToArray());
			header.Write(body.Bytes);
			header.Align(64, 0x70);
			segments.Add(header.Bytes);
		}

		using var index = new StorageBytes();
		for (int i = 0; i < segmentCount; i++) { index.U64(0); index.U32(0); }
		segment("segidx", index);
		using var schemas = new StorageBytes();
		schemas.U32(1); schemas.U32(0);
		schemas.U32(0); schemas.U32(2); schemas.U32(0);
		schemas.U64(0xaf10c); schemas.U32(0); schemas.U32(0);
		schemas.Align(16);
		int namesOffset = (48 + schemas.Length) / 16;
		schemas.U32(1); schemas.Text("AcDb3DSolid_ASM_Data");
		segment("schidx", schemas, namesOffset);

		using var schemaData = new StorageBytes();
		schemaData.U16(0); schemaData.U16(2);
		for (int i = 0; i < 2; i++)
		{
			schemaData.U32(0); schemaData.U32(i); schemaData.U32(i == 0 ? 10 : 15);
			schemaData.U16(0);
		}
		schemaData.Align(16);
		int propertyOffset = (48 + schemaData.Length) / 16;
		schemaData.U32(2); schemaData.Text("AcDbDs::ID"); schemaData.Text("ASM_Data");
		segment("schdat", schemaData, propertyOffset);

		using var dataIndex = new StorageBytes();
		dataIndex.U32(entities.Length); dataIndex.U32(0);
		for (int i = 0; i < entities.Length; i++)
		{
			dataIndex.U32(4); dataIndex.U32(i * 20); dataIndex.U32(0);
		}
		segment("datidx", dataIndex);

		using var data = new StorageBytes();
		int localOffset = 0;
		foreach (var record in records)
		{
			data.U32(20); data.U32(1); data.U64(record.Entity.Handle); data.U32(localOffset);
			localOffset = checked(localOffset + record.Size);
		}
		data.Align(16);
		int objectOffset = (48 + data.Length) / 16;
		foreach (var record in records)
		{
			if (record.Pages.Length == 0)
			{
				data.U32(record.Entity.AcisData.Length); data.Write(record.Entity.AcisData);
			}
			else
			{
				data.U32(0xbb106bb1u);
				data.U64((ulong)record.Entity.AcisData.Length); data.U32(record.Pages.Length);
				data.U32(record.Size); data.U32(pageSize); data.U32(record.Pages[record.Pages.Length - 1].Size);
				data.U32(0); data.U32(0);
				foreach (var page in record.Pages) { data.U32(page.Segment); data.U32(page.Size); }
			}
		}
		segment("_data_", data, 0, objectOffset);

		using var search = new StorageBytes();
		search.U32(1); search.U32(0); search.U64((ulong)entities.Length);
		for (int i = 0; i < entities.Length; i++) search.U64((ulong)i);
		search.U32(1); search.U32(0); search.U32(entities.Length);
		for (int i = 0; i < entities.Length; i++)
		{
			search.U64(entities[i].Handle); search.U64(1); search.U64((ulong)i);
		}
		segment("search", search);
		foreach (var record in records)
		{
			for (int i = 0; i < record.Pages.Length; i++)
			{
				var page = record.Pages[i];
				using var body = new StorageBytes();
				body.U64((ulong)record.Entity.AcisData.Length); body.U64((ulong)page.Offset);
				body.U32(i); body.U32(record.Pages.Length); body.U64((ulong)page.Size);
				body.Write(record.Entity.AcisData, page.Offset, page.Size);
				segment("blob01", body, blob: true);
			}
		}

		using var output = new StorageBytes();
		int size = checked(128 + segments.Sum(s => s.Length));
		foreach (uint field in new uint[] { 0x6472616a, 0x10080, 8, 2, 0, 1, 128, 0, (uint)segments.Count, 1, 3, 5, uint.MaxValue, (uint)size })
			output.U32(field);
		for (int i = 0; i < 4; i++) output.U32(0);
		output.Align(128);
		foreach (byte[] bytes in segments) output.Write(bytes);
		output.Position = 128 + 48;
		ulong offset = 128;
		foreach (byte[] bytes in segments)
		{
			output.U64(offset); output.U32(bytes.Length);
			offset += (ulong)bytes.Length;
		}
		return output.Bytes;
	}

	private sealed class StorageBytes : IDisposable
	{
		private readonly MemoryStream _stream = new();
		private readonly BinaryWriter _writer;
		public StorageBytes() { this._writer = new BinaryWriter(this._stream); }
		public byte[] Bytes => this._stream.ToArray();
		public int Length => checked((int)this._stream.Length);
		public long Position { set => this._stream.Position = value; }
		public void U16(ushort value) => this._writer.Write(value);
		public void U32(int value) => this._writer.Write(value);
		public void U32(uint value) => this._writer.Write(value);
		public void U64(ulong value) => this._writer.Write(value);
		public void Write(byte[] value) => this._writer.Write(value);
		public void Write(byte[] value, int offset, int count) => this._writer.Write(value, offset, count);
		public void Text(string value) { this.Write(Encoding.UTF8.GetBytes(value)); this._writer.Write((byte)0); }
		public void Align(int size, byte fill = 0x55)
		{
			while (this.Length % size != 0) this._writer.Write(fill);
		}
		public void Dispose() => this._writer.Dispose();
	}
}
