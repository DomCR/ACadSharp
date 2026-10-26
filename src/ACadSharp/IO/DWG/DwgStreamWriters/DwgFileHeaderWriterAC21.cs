using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static ACadSharp.IO.DWG.DwgR2007Encoding;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

// R2007 uses an RS-protected container distinct from R2004/R2010.
internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterBase<DwgFileHeaderAC21>
{
	public override int FileHeaderSize => 0x480;
	public override int HandleSectionOffset => 0;
	private sealed class Page { public int Id; public byte[] Bytes; }
	private sealed class Section
	{
		public string Name;
		public byte[] Data;
		public List<(Page Page, int Offset, byte[] Data)> Pages = new();
	}
	private readonly List<Section> sections = new();
	public DwgFileHeaderWriterAC21(Stream stream, Encoding encoding, CadDocument document) : base(stream, encoding, document) { }
	public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400)
		=> sections.Add(new Section { Name = name, Data = stream.ToArray() });
	private static int align(int n, int boundary) => checked((n + boundary - 1) / boundary * boundary);
	private static byte[] pad(byte[] data, int length)
	{
		byte[] result = new byte[length]; Array.Copy(data, result, data.Length); return result;
	}
	private static (byte[] Bytes, int Factor) system(byte[] data)
	{
		byte[] padded = pad(data, align(data.Length, 8));
		int size = Math.Max(0x400, align((padded.Length * 2 + 238) / 239 * 255, 32));
		int factor = size / 255 * 239 / padded.Length;
		byte[] repeated = new byte[padded.Length * factor];
		for (int i = 0; i < factor; i++) Array.Copy(padded, 0, repeated, i * padded.Length, padded.Length);
		return (pad(ReedSolomon(repeated, 239), size), factor);
	}
	public override void WriteFile()
	{
		var pages = new List<Page>();
		Page add(byte[] bytes) { var page = new Page { Id = pages.Count + 1, Bytes = bytes }; pages.Add(page); return page; }
		var placeholder = system(new byte[(sections.Sum(s => (s.Data.Length + 0xf7ff) / 0xf800) + 4) * 16]);
		Page map1 = add(placeholder.Bytes), map2 = add(placeholder.Bytes);
		foreach (var section in sections)
			for (int offset = 0; offset < section.Data.Length; offset += 0xf800)
			{
				byte[] data = new byte[Math.Min(0xf800, section.Data.Length - offset)];
				Array.Copy(section.Data, offset, data, 0, data.Length);
				byte[] encoded = ReedSolomon(pad(data, align(data.Length, 8)), 251);
				section.Pages.Add((add(pad(encoded, align(encoded.Length, 32))), offset, data));
			}
		var hashes = new Dictionary<string, ulong>
		{
			["AcDb:Header"] = 0x32b803d9, ["AcDb:Classes"] = 0x3f54045f, ["AcDb:SummaryInfo"] = 0x717a060f,
			["AcDb:Preview"] = 0x40aa0473, ["AcDb:AppInfo"] = 0x3fa0043e, ["AcDb:FileDepList"] = 0x6c4205ca,
			["AcDb:RevHistory"] = 0x60a205b3, ["AcDb:AcDbObjects"] = 0x674c05a9, ["AcDb:ObjFreeSpace"] = 0x77e2061f,
			["AcDb:Template"] = 0x4a1404ce, ["AcDb:Handles"] = 0x3f6e0450, ["AcDb:AuxHeader"] = 0x54f0050a,
		};
		using var sectionStream = new MemoryStream();
		void chunk(byte[] data) => sectionStream.Write(data, 0, data.Length);
		foreach (var section in sections)
		{
			byte[] name = Encoding.Unicode.GetBytes(section.Name + "\0"); hashes.TryGetValue(section.Name, out ulong hash);
			chunk(Words((ulong)section.Data.Length, 0xf800, 0, hash, (ulong)name.Length, 0, 4, (ulong)section.Pages.Count)); chunk(name);
			foreach (var e in section.Pages)
				chunk(Words((ulong)e.Offset, (ulong)e.Page.Bytes.Length, (ulong)e.Page.Id, (ulong)e.Data.Length, (ulong)e.Data.Length,
					Checksum(e.Data), Crc(e.Data, Seed(e.Data.Length), true)));
		}
		byte[] sectionData = sectionStream.ToArray(); var sectionSystem = system(sectionData);
		Page section1 = add(sectionSystem.Bytes), section2 = add(sectionSystem.Bytes);
		byte[] pageData = Words(pages.SelectMany(p => new[] { (ulong)p.Bytes.Length, (ulong)p.Id }).ToArray());
		var mapSystem = system(pageData); map1.Bytes = map2.Bytes = mapSystem.Bytes;
		var offsets = new Dictionary<int, ulong>(); ulong position = 0;
		foreach (var page in pages) { offsets.Add(page.Id, position); position += (ulong)page.Bytes.Length; }
		var random = new RandomSequence(); ulong sectionSeed = random.EncodeZero(), pageSeed = random.EncodeZero();
		ulong random1 = random.Next(), random2 = random.Next(), encodedSeed = random.EncodeZero();
		var normal = new List<ulong> { Rotate(random1, random2) };
		normal.Add(Rotate(normal[0], normal[0])); normal.Add(Rotate(random2, normal[1]));
		normal.Add(Rotate(normal[2], normal[2])); normal.Add(Rotate(random1, normal[3]));
		while (normal.Count < 8) normal.Add(Rotate(normal[normal.Count - 1], normal[normal.Count - 1]));
		ulong normalCrc = Crc(Words(normal.ToArray()), ~random2);
		var mirrored = new List<ulong> { Rotate(random1, random2) };
		mirrored.Add(Rotate(normalCrc, mirrored[0])); mirrored.Add(Rotate(random2, mirrored[1]));
		mirrored.Add(Rotate(normalCrc, mirrored[2])); mirrored.Add(Rotate(random1, mirrored[3]));
		mirrored.Add(Rotate(normalCrc, mirrored[4])); mirrored.Add(Rotate(random2, mirrored[5])); mirrored.Add(Rotate(mirrored[6], mirrored[6]));
		byte[] check = Words(normalCrc, Crc(Words(mirrored.ToArray()), ~random1, true), random1, random2, encodedSeed);
		ulong pageCrc = Crc(pageData, Seed(pageData.Length), true), sectionCrc = Crc(sectionData, Seed(sectionData.Length), true);
		byte[] payload = Words(0x70, 0x480 + position + 0x400, pageCrc, (ulong)mapSystem.Factor, pageSeed,
			offsets[map2.Id], (ulong)map2.Id, 0, (ulong)map1.Id, position,
			(ulong)pageData.Length, (ulong)pageData.Length, (ulong)pages.Count, (ulong)pages.Count, 0x20, 0x40,
			pageCrc, 0xf800, 4, 1, (ulong)sections.Count + 1, sectionCrc, (ulong)sectionData.Length,
			(ulong)section2.Id, (ulong)section1.Id, (ulong)sectionData.Length, sectionCrc,
			(ulong)sectionSystem.Factor, sectionSeed, 0x60100, 0, random.EncodeZero(), 0, 0);
		Array.Copy(Words(Crc(payload, Seed(payload.Length, true))), 0, payload, 0x108, 8);
		ulong key = random.Next();
		byte[] prefix = Words(Crc(Words(key, Rotate(key, key)), Seed(16)), key, Crc(payload, Seed(payload.Length, true)), unchecked((ulong)-payload.Length));
		byte[] block = prefix.Concat(payload).ToArray(), repeated = new byte[717];
		for (int p = 0; p + block.Length <= repeated.Length; p += block.Length) Array.Copy(block, 0, repeated, p, block.Length);
		byte[] header = pad(ReedSolomon(repeated, 239), 0x400); Array.Copy(check, 0, header, 0x3d8, check.Length);
		byte[] metadata = new byte[0x80]; Array.Copy(Encoding.ASCII.GetBytes("AC1021"), metadata, 6);
		metadata[0xb] = 50; metadata[0xc] = 3; metadata[0x11] = 0x1b; metadata[0x12] = 50;
		using (var writer = new BinaryWriter(new MemoryStream(metadata)))
		{
			writer.BaseStream.Position = 0x13; writer.Write(this.getFileCodePage()); writer.BaseStream.Position = 0x28; writer.Write(0x80u);
			foreach (var entry in new[] { ("AcDb:Preview", 0xd), ("AcDb:SummaryInfo", 0x20), ("AcDb:AppInfo", 0x2c) })
			{
				var page = sections.FirstOrDefault(s => s.Name == entry.Item1)?.Pages.FirstOrDefault().Page;
				if (page == null) continue;
				writer.BaseStream.Position = entry.Item2; writer.Write(checked((uint)(0x480 + offsets[page.Id])));
			}
		}
		this._stream.Position = 0; this._stream.Write(metadata, 0, metadata.Length); this._stream.Write(header, 0, header.Length);
		foreach (var page in pages) this._stream.Write(page.Bytes, 0, page.Bytes.Length);
		this._stream.Write(header, 0, header.Length);
	}
}
