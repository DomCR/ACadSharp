using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ACadSharp.IO.DWG;

// Pre-R13 DWG uses byte-aligned records. Reuse the R12 DXF model builder for
// ownership and sequences; DWG framing and primitive encoding stay independent.
internal sealed class DwgR12Codec
{
	private sealed class Record : List<(int Code, string Value)>
	{
		public Record() { }
		public Record(params (int Code, string Value)[] pairs) : base(pairs) { }
		public string Value(int code, string fallback = "") => this.FirstOrDefault(p => p.Code == code).Value ?? fallback;
		public double Number(int code, double fallback = 0) => double.Parse(Value(code, str(fallback)), CultureInfo.InvariantCulture);
		public bool Has(int code) => this.Any(p => p.Code == code);
	}
	private static string str(double value) => value.ToString("R", CultureInfo.InvariantCulture);
	private static readonly Dictionary<string, int> types = new()
	{
		["LINE"] = 1, ["POINT"] = 2, ["CIRCLE"] = 3, ["SHAPE"] = 4, ["TEXT"] = 7,
		["ARC"] = 8, ["TRACE"] = 9, ["SOLID"] = 11, ["BLOCK"] = 12, ["ENDBLK"] = 13,
		["INSERT"] = 14, ["ATTDEF"] = 15, ["ATTRIB"] = 16, ["SEQEND"] = 17,
		["POLYLINE"] = 19, ["VERTEX"] = 20, ["3DFACE"] = 22,
	};
	private static readonly Dictionary<string, int> tableHeaders = new()
	{
		["BLOCK"] = 44, ["LAYER"] = 54, ["STYLE"] = 64, ["LTYPE"] = 74, ["VIEW"] = 84,
		["UCS"] = 0x3ef, ["VPORT"] = 0x500, ["APPID"] = 0x512, ["DIMSTYLE"] = 0x522, ["VX"] = 0x69f,
	};
	private static readonly (string Name, int Offset, int[] Codes, string Type)[] headerFields =
	{
		("$INSBASE", 0x5e, new[] { 10, 20, 30 }, "double"), ("$PLINEGEN", 0x76, new[] { 70 }, "short"),
		("$EXTMIN", 0x78, new[] { 10, 20, 30 }, "double"), ("$EXTMAX", 0x90, new[] { 10, 20, 30 }, "double"),
		("$LIMMIN", 0xa8, new[] { 10, 20 }, "double"), ("$LIMMAX", 0xb8, new[] { 10, 20 }, "double"),
		("$ORTHOMODE", 0x128, new[] { 70 }, "short"), ("$FILLMODE", 0x12c, new[] { 70 }, "short"),
		("$LTSCALE", 0x132, new[] { 40 }, "double"), ("$TEXTSIZE", 0x13a, new[] { 40 }, "double"),
		("$TRACEWID", 0x142, new[] { 40 }, "double"),
	};
	private static readonly ConditionalWeakTable<CadDocument, byte[]> originalHeaders = new();
	private readonly Dictionary<string, List<Record>> tables = new();
	private readonly List<int> blockOffsets = new();
	private readonly HashSet<ulong> writtenHandles = new();
	private readonly Encoding encoding;
	private ulong nextHandle = 1;
	private bool checkCrc;
	public DwgR12Codec(Encoding encoding) { this.encoding = encoding; }
	private List<Record> rows(string name) => tables.TryGetValue(name, out var result) ? result : new List<Record>();
	private Record row(string name, int index) => index >= 0 && index < rows(name).Count ? rows(name)[index] : new Record();
	private static byte[] slice(byte[] data, int start, int length)
	{
		if (start < 0 || length < 0 || start > data.Length - length) throw new InvalidDataException("Invalid R12 DWG section bounds.");
		byte[] result = new byte[length]; Array.Copy(data, start, result, 0, length); return result;
	}
	private static ushort u16(byte[] data, int offset) => (ushort)(data[offset] | data[offset + 1] << 8);
	private static uint u32(byte[] data, int offset) => (uint)(u16(data, offset) | (uint)u16(data, offset + 2) << 16);
	private static void put(byte[] data, int offset, ulong value, int size)
	{
		for (int i = 0; i < size; i++) data[offset + i] = (byte)(value >> (i * 8));
	}
	private static byte[] checkedRecord(byte[] data)
	{
		byte[] result = new byte[data.Length + 2]; Array.Copy(data, result, data.Length);
		put(result, data.Length, CRC.ApplyCrc8(0xc0c1, data), 2); return result;
	}
	private sealed class Bytes
	{
		public readonly byte[] Data;
		public int Position;
		private readonly Encoding encoding;
		public Bytes(byte[] data, Encoding encoding) { Data = data; this.encoding = encoding; }
		private void require(int count)
		{
			if (Position < 0 || count < 0 || Position > Data.Length - count) throw new InvalidDataException("Truncated R12 record.");
		}
		public string Read(string type)
		{
			int size = type == "byte" ? 1 : type == "short" || type == "text" ? 2 : type == "long" ? 4 : 8;
			require(size); int p = Position; Position += size;
			switch (type)
			{
				case "byte": return str(Data[p]);
				case "short": return str(unchecked((short)u16(Data, p)));
				case "long": return str(u32(Data, p));
				case "double":
					byte[] bytes = slice(Data, p, 8); if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					return str(BitConverter.ToDouble(bytes, 0));
				case "text":
					int length = u16(Data, p); require(length);
					string text = encoding.GetString(Data, Position, length); Position += length; return text;
				default: throw new ArgumentException(nameof(type));
			}
		}
		public void Write(string type, string value)
		{
			byte[] bytes;
			if (type == "text")
			{
				bytes = encoding.GetBytes(value);
				if (bytes.Length > ushort.MaxValue) throw new InvalidDataException("R12 text exceeds 65535 bytes.");
				require(bytes.Length + 2); put(Data, Position, (ulong)bytes.Length, 2); Position += 2;
			}
			else
			{
				double number = double.Parse(value, CultureInfo.InvariantCulture);
				if (type == "double") { bytes = BitConverter.GetBytes(number); if (!BitConverter.IsLittleEndian) Array.Reverse(bytes); }
				else
				{
					bytes = new byte[type == "byte" ? 1 : type == "short" ? 2 : 4];
					put(bytes, 0, unchecked((ulong)(long)number), bytes.Length);
				}
				require(bytes.Length);
			}
			Array.Copy(bytes, 0, Data, Position, bytes.Length); Position += bytes.Length;
		}
	}
	private void field(Record record, Bytes bytes, bool read, int code, string type = "double", string reference = null)
	{
		if (read)
		{
			string v = bytes.Read(type);
			if (type == "double" && code >= 50 && code <= 58) v = str(double.Parse(v, CultureInfo.InvariantCulture) * 180 / Math.PI);
			if (reference != null)
			{
				int index = int.Parse(v, CultureInfo.InvariantCulture);
				v = reference == "LTYPE" && index < 0 ? index == -2 ? "BYBLOCK" : "BYLAYER"
					: row(reference, index).Value(2, reference == "LTYPE" ? "BYLAYER" : "STANDARD");
			}
			record.Add((code, v));
		}
		else
		{
			string v = record.Value(code, type == "text" ? "" : "0");
			if (type == "double" && code >= 50 && code <= 58) v = str(double.Parse(v, CultureInfo.InvariantCulture) * Math.PI / 180);
			if (reference != null)
			{
				int index = rows(reference).FindIndex(r => r.Value(2).Equals(v, StringComparison.OrdinalIgnoreCase));
				if (index < 0 && reference != "LTYPE") throw new InvalidDataException("Missing R12 " + reference + " reference: " + v);
				v = str(index < 0 && reference == "LTYPE" && v.Equals("BYBLOCK", StringComparison.OrdinalIgnoreCase) ? -2 : index);
			}
			bytes.Write(type, v);
		}
	}
	private int geometry(string name, Record record, Bytes bytes, bool read, int opts, int flags)
	{
		void f(int code, string type = "double", string reference = null) => field(record, bytes, read, code, type, reference);
		void point(int code, int dimensions = 2) { for (int i = 0; i < dimensions; i++) f(code + 10 * i); }
		void optional(int bit, int code, string type = "double", string reference = null)
		{
			if (!read && record.Has(code)) opts |= bit;
			if ((opts & bit) != 0) f(code, type, reference);
		}
		void extrusion(int bit)
		{
			if (!read && new[] { 210, 220, 230 }.Any(record.Has)) opts |= bit;
			if ((opts & bit) != 0) point(210, 3);
		}
		switch (name)
		{
			case "LINE": point(10, (flags & 4) != 0 ? 2 : 3); point(11, (flags & 4) != 0 ? 2 : 3); extrusion(1); break;
			case "POINT": point(10, (flags & 4) != 0 ? 2 : 3); extrusion(1); optional(2, 50); break;
			case "CIRCLE": case "ARC":
				point(10); f(40); if (name == "ARC") { f(50); f(51); } extrusion(1); optional(2, 30); break;
			case "TEXT": case "ATTRIB": case "ATTDEF":
				point(10); f(40); f(1, "text"); int shift = name == "TEXT" ? 1 : 2;
				if (name == "ATTDEF") f(3, "text");
				if (name != "TEXT") { f(2, "text"); f(70, "byte"); }
				optional(shift, 50); optional(shift * 2, 41); optional(shift * 4, 51);
				optional(shift * 8, 7, "byte", "STYLE"); optional(shift * 16, 71, "byte"); optional(shift * 32, 72, "byte");
				if (!read && record.Has(11)) opts |= shift * 64;
				if ((opts & (shift * 64)) != 0) point(11);
				extrusion(shift * 128); optional(shift * 256, name == "TEXT" ? 73 : 74, "byte"); break;
			case "BLOCK": point(10); optional(2, 1, "text"); optional(4, 2, "text"); break;
			case "ENDBLK": break;
			case "SEQEND": if (read) bytes.Read("long"); else bytes.Write("long", "0"); break;
			case "INSERT":
				f(2, "short", "BLOCK"); point(10);
				foreach (var pair in new[] { (1, 41), (2, 42), (4, 50), (8, 43), (16, 70), (32, 71), (64, 44), (128, 45) })
					optional(pair.Item1, pair.Item2, pair.Item1 == 16 || pair.Item1 == 32 ? "short" : "double");
				extrusion(256); if (read && (flags & 128) != 0) record.Add((66, "1")); break;
			case "POLYLINE":
				optional(1, 70, "byte"); optional(2, 40); optional(4, 41); extrusion(8);
				foreach (var pair in new[] { (16, 71), (32, 72), (64, 73), (128, 74), (256, 75) }) optional(pair.Item1, pair.Item2, "short");
				if (read) record.AddRange(new[] { (66, "1"), (10, "0"), (20, "0") }); break;
			case "VERTEX":
				if (!read && record.Number(70) == 128) opts |= 0x4000;
				if ((opts & 0x4000) == 0) point(10);
				optional(1, 40); optional(2, 41); optional(4, 42); optional(8, 70, "byte"); optional(16, 50);
				for (int i = 0; i < 4; i++) optional(32 << i, 71 + i, "short"); break;
			case "SOLID": case "TRACE": for (int c = 10; c < 14; c++) point(c); extrusion(1); break;
			case "3DFACE": for (int c = 10; c < 14; c++) point(c, (flags & 4) != 0 ? 2 : 3); optional(1, 70, "short"); break;
			default: throw new InvalidDataException("Unsupported R12 DWG entity: " + name);
		}
		return opts;
	}
	private byte[] encode(Record record)
	{
		string name = record.Value(0);
		if (!types.TryGetValue(name, out int type)) throw new InvalidDataException("Unsupported R12 DWG entity: " + name);
		var bytes = new Bytes(new byte[0x10000], encoding) { Position = 8 };
		int flags = 32;
		if (record.Has(62)) flags |= 1;
		if (record.Has(6) && !record.Value(6).Equals("BYLAYER", StringComparison.OrdinalIgnoreCase)) flags |= 2;
		if (!new[] { "LINE", "POINT", "3DFACE" }.Contains(name) && record.Number(30) != 0) flags |= 4;
		if (record.Number(39) != 0) flags |= 8;
		if (record.Number(67) != 0) flags |= 64;
		if (record.Number(66) != 0 || name == "POLYLINE") flags |= 128;
		if ((flags & 64) != 0) bytes.Write("byte", "0");
		if ((flags & 1) != 0) bytes.Write("byte", record.Value(62));
		if ((flags & 2) != 0) field(record, bytes, false, 6, "short", "LTYPE");
		if ((flags & 4) != 0) bytes.Write("double", record.Value(30));
		if ((flags & 8) != 0) bytes.Write("double", record.Value(39));
		ulong.TryParse(record.Value(5, "0"), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong handle);
		if (handle == 0 || writtenHandles.Contains(handle)) handle = nextHandle++;
		writtenHandles.Add(handle);
		string hex = handle.ToString("X", CultureInfo.InvariantCulture); if (hex.Length % 2 != 0) hex = "0" + hex;
		bytes.Write("byte", str(hex.Length / 2));
		for (int i = 0; i < hex.Length; i += 2) bytes.Write("byte", str(Convert.ToByte(hex.Substring(i, 2), 16)));
		int opts = geometry(name, record, bytes, false, 0, flags);
		if (bytes.Position + 2 > ushort.MaxValue) throw new InvalidDataException("R12 entity record exceeds 65535 bytes.");
		bytes.Data[0] = (byte)type; bytes.Data[1] = (byte)flags;
		put(bytes.Data, 2, (ulong)bytes.Position + 2, 2);
		int layer = rows("LAYER").FindIndex(r => r.Value(2) == record.Value(8, "0"));
		if (layer < 0) throw new InvalidDataException("Missing R12 layer reference.");
		put(bytes.Data, 4, (ulong)layer, 2); put(bytes.Data, 6, (ulong)opts, 2);
		return checkedRecord(slice(bytes.Data, 0, bytes.Position));
	}
	private static void patchSequences(List<Record> records, List<byte[]> encoded, int start)
	{
		int offset = start, begin = start;
		for (int i = 0; i < records.Count; i++)
		{
			string name = records[i].Value(0); byte[] bytes = encoded[i];
			if (name == "POLYLINE" || name == "INSERT") begin = offset;
			if (name == "SEQEND")
			{
				put(bytes, bytes.Length - 6, (ulong)begin, 4);
				put(bytes, bytes.Length - 2, CRC.ApplyCrc8(0xc0c1, slice(bytes, 0, bytes.Length - 2)), 2);
			}
			offset += bytes.Length;
		}
	}
	private List<Record> decode(byte[] data, bool strict, Action<string> warn)
	{
		var result = new List<Record>();
		for (int offset = 0; offset < data.Length;)
		{
			if (offset + 4 > data.Length) throw new InvalidDataException("Truncated R12 entity header.");
			int size = u16(data, offset + 2);
			if (size < 10 || offset > data.Length - size) throw new InvalidDataException("Invalid R12 entity record size.");
			byte[] raw = slice(data, offset, size); offset += size;
			if (checkCrc && CRC.ApplyCrc8(0xc0c1, slice(raw, 0, size - 2)) != u16(raw, size - 2)) throw new InvalidDataException("R12 entity CRC mismatch.");
			if ((raw[0] & 128) != 0) continue;
			string name = types.FirstOrDefault(t => t.Value == (raw[0] & 127)).Key;
			if (name == null) { if (strict) throw new InvalidDataException("Unsupported R12 DWG entity type " + raw[0]); warn("Unsupported R12 DWG entity type " + raw[0]); continue; }
			var record = new Record((0, name), (8, row("LAYER", u16(raw, 4)).Value(2, "0")));
			var bytes = new Bytes(slice(raw, 0, size - 2), encoding) { Position = 8 };
			int flags = raw[1], opts = u16(raw, 6);
			try
			{
				int extra = 0;
				if ((flags & 64) != 0) { extra = int.Parse(bytes.Read("byte"), CultureInfo.InvariantCulture); record.Add((67, "1")); }
				if ((extra & 2) != 0) { int length = int.Parse(bytes.Read("short"), CultureInfo.InvariantCulture); if (length < 0) throw new InvalidDataException("Invalid R12 extended data length."); bytes.Position += length; warn("R12 extended entity data is not retained."); }
				if ((flags & 1) != 0) field(record, bytes, true, 62, "byte");
				if ((flags & 2) != 0) field(record, bytes, true, 6, "short", "LTYPE");
				if ((flags & 4) != 0 && !new[] { "LINE", "POINT", "3DFACE" }.Contains(name)) field(record, bytes, true, 30);
				if ((flags & 8) != 0) field(record, bytes, true, 39);
				if ((flags & 32) != 0)
				{
					int length = int.Parse(bytes.Read("byte"), CultureInfo.InvariantCulture); string handle = "";
					if (length > 8) throw new InvalidDataException("Invalid R12 handle length.");
					for (int i = 0; i < length; i++) handle += int.Parse(bytes.Read("byte"), CultureInfo.InvariantCulture).ToString("X2");
					record.Add((5, handle));
				}
				if ((extra & 4) != 0) bytes.Read("short");
				if (name == "POLYLINE" && (opts & 0x8000) != 0) throw new InvalidDataException("R12 overflow polylines are unsupported.");
				geometry(name, record, bytes, true, opts, flags);
				if (bytes.Position > size - 2) throw new InvalidDataException("Truncated R12 " + name + " record.");
				result.Add(record);
			}
			catch (Exception e) when (!strict && (e is InvalidDataException || e is FormatException || e is IndexOutOfRangeException)) { warn(e.Message); }
		}
		return result;
	}
	public CadDocument Read(byte[] data, bool strict, Action<string> warn, bool checkCrc = false)
	{
		this.checkCrc = checkCrc;
		if (data.Length < 0x6bf) throw new InvalidDataException("Truncated R12 DWG header.");
		int crcPosition = checked((int)u32(data, 20) - 18);
		if (crcPosition != 0x6bd && crcPosition != 0x6bb) throw new InvalidDataException("Unsupported R12 header layout.");
		if (checkCrc && CRC.ApplyCrc8(0xc0c1, slice(data, 0, crcPosition)) != u16(data, crcPosition)) throw new InvalidDataException("R12 header CRC mismatch.");
		string fixedText(byte[] raw, int p, int n) => encoding.GetString(slice(raw, p, n)).Split('\0')[0];
		foreach (var entry in tableHeaders)
		{
			string name = entry.Key; int h = entry.Value;
			int size = u16(data, h), count = u16(data, h + 2), start = checked((int)u32(data, h + 6));
			var records = new List<Record>(); tables.Add(name, records);
			if (count > 0 && size < 35) throw new InvalidDataException("Invalid R12 table record size.");
			byte[] table = slice(data, start, checked(count * size));
			for (int i = 0; i < count; i++)
			{
				byte[] raw = slice(table, i * size, size);
				if (checkCrc && CRC.ApplyCrc8(0xc0c1, slice(raw, 0, size - 2)) != u16(raw, size - 2)) throw new InvalidDataException("R12 " + name + " CRC mismatch.");
				var record = new Record((0, name), (2, fixedText(raw, 1, 32)), (70, str(raw[0] & 127)));
				if (name == "BLOCK")
				{
					string blockName = record.Value(2);
					if (Regex.IsMatch(blockName, @"^\*[A-Z]$", RegexOptions.IgnoreCase) || records.Any(r => r.Value(2) == blockName)) blockName += i;
					record[1] = (2, blockName);
				}
				var bytes = new Bytes(raw, encoding) { Position = 35 };
				if (name == "BLOCK") blockOffsets.Add(checked((int)(u32(raw, 35) & 0x3fffffff)));
				if (name == "LAYER") { field(record, bytes, true, 62, "short"); field(record, bytes, true, 6, "short", "LTYPE"); }
				if (name == "STYLE")
				{
					foreach (int c in new[] { 40, 41, 50 }) field(record, bytes, true, c);
					field(record, bytes, true, 71, "byte"); field(record, bytes, true, 42);
					record.Add((3, fixedText(raw, 68, 64))); record.Add((4, fixedText(raw, 132, 64)));
				}
				if (name == "LTYPE")
				{
					record.AddRange(new[] { (3, fixedText(raw, 35, 48)), (72, str(raw[83])), (73, str(raw[84])) });
					bytes.Position = 85; field(record, bytes, true, 40);
					for (int j = 0; j < Math.Min((int)raw[84], 12); j++) field(record, bytes, true, 49);
				}
				records.Add((raw[0] & 128) == 0 ? record : new Record((0, name), (2, "")));
			}
		}
		// Layer references precede linetypes on disk.
		for (int i = 0; i < rows("LAYER").Count; i++)
		{
			int h = tableHeaders["LAYER"], p = checked((int)u32(data, h + 6) + i * u16(data, h) + 37);
			var record = rows("LAYER")[i]; int index = record.FindIndex(pair => pair.Code == 6);
			if (index >= 0) record[index] = (6, row("LTYPE", u16(data, p)).Value(2, "CONTINUOUS"));
		}
		int modelStart = checked((int)u32(data, 20)), modelEnd = checked((int)u32(data, 24));
		var model = decode(slice(data, modelStart, modelEnd - modelStart), strict, warn);
		int blockStart = checked((int)u32(data, 28)), blockSize = (int)(u32(data, 32) & 0x3fffffff);
		var blocks = new List<Record>();
		for (int i = 0; i < blockOffsets.Count; i++)
		{
			int start = blockOffsets[i], end = i + 1 < blockOffsets.Count ? blockOffsets[i + 1] : blockSize;
			if (row("BLOCK", i).Value(2).Length == 0) continue;
			var records = decode(slice(data, checked(blockStart + start), end - start), strict, warn);
			var begin = records.FirstOrDefault(r => r.Value(0) == "BLOCK");
			if (begin == null) continue;
			begin.RemoveAll(p => p.Code == 2); begin.Add((2, row("BLOCK", i).Value(2))); blocks.AddRange(records);
		}
		var pairs = new Record((0, "SECTION"), (2, "HEADER"), (9, "$ACADVER"), (1, "AC1009"), (9, "$DWGCODEPAGE"), (3, "ANSI_" + encoding.CodePage));
		foreach (var header in headerFields)
		{
			var bytes = new Bytes(data, encoding) { Position = header.Offset }; pairs.Add((9, header.Name));
			foreach (int code in header.Codes) pairs.Add((code, bytes.Read(header.Type)));
		}
		pairs.AddRange(new[] { (0, "ENDSEC"), (0, "SECTION"), (2, "TABLES") });
		foreach (var table in tables)
		{
			if (!new[] { "LAYER", "LTYPE", "STYLE", "APPID" }.Contains(table.Key)) continue;
			pairs.AddRange(new[] { (0, "TABLE"), (2, table.Key), (70, str(table.Value.Count)) });
			pairs.AddRange(table.Value.Where(r => r.Value(2).Length > 0).SelectMany(r => r)); pairs.Add((0, "ENDTAB"));
		}
		pairs.AddRange(new[] { (0, "ENDSEC"), (0, "SECTION"), (2, "BLOCKS") }); pairs.AddRange(blocks.SelectMany(r => r));
		pairs.AddRange(new[] { (0, "ENDSEC"), (0, "SECTION"), (2, "ENTITIES") }); pairs.AddRange(model.SelectMany(r => r));
		pairs.AddRange(new[] { (0, "ENDSEC"), (0, "EOF") });
		string text = string.Concat(pairs.Select(p => p.Code.ToString(CultureInfo.InvariantCulture) + "\n" + p.Value + "\n"));
		using var reader = new DxfReader(new MemoryStream(encoding.GetBytes(text)));
		reader.Configuration.Failsafe = !strict;
		reader.OnNotification += (_, e) => warn(e.Message);
		CadDocument document = reader.Read(); document.Header.Version = ACadVersion.AC1009;
		originalHeaders.Add(document, slice(data, 0, crcPosition)); return document;
	}

	public byte[] Write(CadDocument document, Action<string> warn)
	{
		using var dxf = new MemoryStream();
		DxfWriter.Write(dxf, document, false, new DxfWriterConfiguration { CloseStream = false }, (_, e) => warn(e.Message));
		string[] lines = encoding.GetString(dxf.ToArray()).Replace("\r\n", "\n").Split('\n');
		var records = new List<Record>(); var current = new Record();
		for (int i = 0; i + 1 < lines.Length; i += 2)
		{
			int code = int.Parse(lines[i], CultureInfo.InvariantCulture);
			if (code == 0) { if (current.Count > 0) records.Add(current); current = new Record(); }
			current.Add((code, lines[i + 1]));
		}
		if (current.Count > 0) records.Add(current);
		if (records.Any(r => r.Any(p => p.Code >= 1000))) warn("R12 DWG extended entity data is not retained.");
		nextHandle = records.SelectMany(r => r).Where(p => p.Code == 5).Select(p => ulong.TryParse(p.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong h) ? h : 0).DefaultIfEmpty(0UL).Max() + 1;
		var sections = new Dictionary<string, List<Record>>(); string section = "", table = "";
		foreach (var record in records)
		{
			string name = record.Value(0);
			if (name == "SECTION") { section = record.Value(2); sections[section] = new List<Record>(); }
			else if (name == "TABLE") { table = record.Value(2); tables[table] = new List<Record>(); }
			else if (name == "ENDTAB") table = "";
			else if (name != "ENDSEC" && name != "EOF")
			{
				if (table.Length > 0) tables[table].Add(record);
				else if (sections.TryGetValue(section, out var list)) list.Add(record);
			}
		}
		List<Record> sectionRows(string name) => sections.TryGetValue(name, out var result) ? result : new List<Record>();
		var blockRecords = sectionRows("BLOCKS"); tables["BLOCK"] = blockRecords.Where(r => r.Value(0) == "BLOCK").ToList();
		tables["LTYPE"] = rows("LTYPE").Where(r => !new[] { "BYLAYER", "BYBLOCK" }.Contains(r.Value(2).ToUpperInvariant())).ToList();
		byte[] template = DwgR12Template.Create(), header = slice(template, 0, 0x6cf);
		if (originalHeaders.TryGetValue(document, out var original)) Array.Copy(original, header, original.Length);
		put(header, 17, 205, 2); put(header, 20, (ulong)header.Length, 4);
		var headerRecord = records.FirstOrDefault(r => r.Value(0) == "SECTION" && r.Value(2) == "HEADER") ?? new Record();
		foreach (var field in headerFields)
		{
			int start = headerRecord.FindIndex(p => p.Code == 9 && p.Value == field.Name); if (start < 0) continue;
			int end = start + 1; while (end < headerRecord.Count && headerRecord[end].Code != 9) end++;
			var record = new Record(); record.AddRange(headerRecord.Skip(start + 1).Take(end - start - 1));
			var bytes = new Bytes(header, encoding) { Position = field.Offset };
			foreach (int code in field.Codes) bytes.Write(field.Type, record.Value(code, "0"));
		}
		using var result = new MemoryStream(); result.Write(header, 0, header.Length);
		void append(byte[] data) => result.Write(data, 0, data.Length);
		// The C# document creates a paper-size viewport by default. It is not
		// a geometry record in this limited pre-R13 codec.
		var entities = sectionRows("ENTITIES").Where(r => r.Value(0) != "VIEWPORT" || r.Number(69) != 1 || r.Number(67) != 1).ToList();
		var encodedEntities = entities.Select(encode).ToList();
		patchSequences(entities, encodedEntities, header.Length);
		foreach (var bytes in encodedEntities) append(bytes);
		int entityEnd = checked((int)result.Position); put(header, 24, (ulong)entityEnd, 4); append(slice(template, 0x6cf, 16));
		var blockBytes = new List<byte[]>(); int blockOffset = 0;
		foreach (var record in blockRecords)
		{
			if (record.Value(0) == "BLOCK") blockOffsets.Add(blockOffset);
			byte[] encoded = encode(record); blockBytes.Add(encoded); blockOffset += encoded.Length;
		}
		foreach (var entry in tableHeaders.OrderBy(t => u32(template, t.Value + 6)))
		{
			string name = entry.Key; int h = entry.Value;
			int oldStart = (int)u32(template, h + 6), size = u16(template, h), oldCount = u16(template, h + 2);
			append(slice(template, oldStart - 16, 16)); put(header, h + 6, (ulong)result.Position, 4);
			if (tables.TryGetValue(name, out var tableRows) && new[] { "BLOCK", "LAYER", "STYLE", "LTYPE", "APPID" }.Contains(name))
			{
				if (tableRows.Count > 32767) throw new InvalidDataException("R12 table exceeds 32767 records.");
				put(header, h + 2, (ulong)tableRows.Count, 2);
				for (int i = 0; i < tableRows.Count; i++)
				{
					var record = tableRows[i]; byte[] raw = new byte[size - 2]; var bytes = new Bytes(raw, encoding);
					byte[] nameBytes = encoding.GetBytes(record.Value(2));
					if (nameBytes.Length > 31) throw new InvalidDataException("R12 table names must fit in 31 bytes.");
					raw[0] = (byte)record.Number(70); Array.Copy(nameBytes, 0, raw, 1, nameBytes.Length);
					bytes.Position = 33; bytes.Write("short", "-1");
					void fixedText(string value, int p, int n) { byte[] b = encoding.GetBytes(value); Array.Copy(b, 0, raw, p, Math.Min(b.Length, n)); }
					if (name == "BLOCK") { bytes.Write("long", str(blockOffsets[i])); bytes.Write("short", "0"); bytes.Write("short", "0"); }
					if (name == "LAYER") { field(record, bytes, false, 62, "short"); field(record, bytes, false, 6, "short", "LTYPE"); }
					if (name == "STYLE")
					{
						foreach (int c in new[] { 40, 41, 50 }) field(record, bytes, false, c);
						field(record, bytes, false, 71, "byte"); field(record, bytes, false, 42);
						fixedText(record.Value(3), 68, 63); fixedText(record.Value(4), 132, 63);
					}
					if (name == "LTYPE")
					{
						fixedText(record.Value(3), 35, 47); bytes.Position = 83;
						field(record, bytes, false, 72, "byte"); field(record, bytes, false, 73, "byte"); field(record, bytes, false, 40);
						var dashes = record.Where(p => p.Code == 49).ToArray(); if (dashes.Length > 12) throw new InvalidDataException("R12 linetypes support at most 12 dashes.");
						foreach (var dash in dashes) bytes.Write("double", dash.Value);
					}
					append(checkedRecord(raw));
				}
			}
			else append(slice(template, oldStart, size * oldCount));
			append(slice(template, oldStart + size * oldCount, 16));
		}
		int oldBlock = (int)u32(template, 28); append(slice(template, oldBlock - 16, 16));
		put(header, 28, (ulong)result.Position, 4); patchSequences(blockRecords, blockBytes, checked((int)result.Position));
		foreach (var bytes in blockBytes) append(bytes);
		put(header, 32, 0x40000000UL + (ulong)blockOffset, 4); append(slice(template, oldBlock, 16));
		int oldExtraEnd = template.Length - 170; append(slice(template, oldBlock + 16, oldExtraEnd - oldBlock - 16));
		put(header, 36, (ulong)result.Position - 16, 4); int auxStart = checked((int)result.Position);
		byte[] aux = slice(template, oldExtraEnd, template.Length - oldExtraEnd);
		put(header, 0x4ee, nextHandle, 8); put(aux, 38, nextHandle, 8);
		put(aux, 20, (ulong)header.Length, 4); put(aux, 24, (ulong)entityEnd, 4);
		put(aux, 28, u32(header, 28), 4); put(aux, 32, u32(header, 36), 4);
		string[] auxTables = { "BLOCK", "LTYPE", "LAYER", "STYLE", "VIEW", "UCS", "VPORT", "APPID", "DIMSTYLE", "VX" };
		for (int i = 0; i < auxTables.Length; i++)
		{
			int h = tableHeaders[auxTables[i]], p = 48 + i * 10;
			put(aux, p + 2, u16(header, h), 2); put(aux, p + 4, u16(header, h + 2), 2); put(aux, p + 6, u32(header, h + 6), 4);
		}
		put(aux, aux.Length - 22, (ulong)auxStart, 4);
		put(aux, aux.Length - 18, CRC.ApplyCrc8(0xc0c1, slice(aux, 16, aux.Length - 34)), 2);
		put(header, 0x6bd, CRC.ApplyCrc8(0xc0c1, slice(header, 0, 0x6bd)), 2);
		append(aux); result.Position = 0; result.Write(header, 0, header.Length); return result.ToArray();
	}
}
