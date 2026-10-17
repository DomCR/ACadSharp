using ACadSharp.IO.DWG.FileHeaders;
using CSUtilities.Converters;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgFileHeaderWriterAC15 : DwgFileHeaderWriterBase<DwgFileHeaderAC15>
{
	/// <summary>
	/// R14 (AC1014) has a 5-record locator and no AuxHeader section;
	/// R2000 (AC1015) has 6 records including the AuxHeader.
	/// </summary>
	private bool IsR14 { get { return this._version == ACadVersion.AC1014; } }

	public override int FileHeaderSize { get { return this.IsR14 ? 0x58 : 0x61; } }

	public override int HandleSectionOffset
	{
		get
		{
			return (int)(FileHeaderSize
				+ (this._records[DwgSectionDefinition.AuxHeader].Stream?.Length ?? 0)
				+ this._records[DwgSectionDefinition.Preview].Stream.Length
				+ this._records[DwgSectionDefinition.Header].Stream.Length
				+ this._records[DwgSectionDefinition.Classes].Stream.Length);
		}
	}

	private const int _nRecordsR2000 = 6;
	private const int _nRecordsR14 = 5;

	private byte[] _endSentinel = new byte[16]
	{
		0x95,0xA0,0x4E,0x28,0x99,0x82,0x1A,0xE5,0x5E,0x41,0xE0,0x5F,0x9D,0x3A,0x4D,0x00
	};

	//Second header section sentinels (R13-R2000), see ODA spec chapter 25
	private static readonly byte[] _secondHeaderBeginSentinel = new byte[16]
	{
		0xD4,0x7B,0x21,0xCE,0x28,0x93,0x9F,0xBF,0x53,0x24,0x40,0x09,0x12,0x3C,0xAA,0x01
	};

	private static readonly byte[] _secondHeaderEndSentinel = new byte[16]
	{
		0x2B,0x84,0xDE,0x31,0xD7,0x6C,0x60,0x40,0xAC,0xDB,0xBF,0xF6,0xED,0xC3,0x55,0xFE
	};

	private Dictionary<string, DwgSectionLocatorRecord> _records = new();

	public DwgFileHeaderWriterAC15(
		System.IO.Stream stream,
		Encoding encoding,
		CadDocument document)
		: base(stream, encoding, document)
	{
		this._records = new Dictionary<string, DwgSectionLocatorRecord>
		{
			{ DwgSectionDefinition.AuxHeader   , new DwgSectionLocatorRecord(5) },
			{ DwgSectionDefinition.Preview     , new DwgSectionLocatorRecord(null) },
			{ DwgSectionDefinition.Header      , new DwgSectionLocatorRecord(0) },
			{ DwgSectionDefinition.Classes     , new DwgSectionLocatorRecord(1) },
			{ DwgSectionDefinition.AcDbObjects , new DwgSectionLocatorRecord(null) },
			{ DwgSectionDefinition.Handles     , new DwgSectionLocatorRecord(2) },
			{ DwgSectionDefinition.ObjFreeSpace, new DwgSectionLocatorRecord(3) },
			{ DwgSectionDefinition.Template    , new DwgSectionLocatorRecord(4) },
		};
	}

	public override void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 29696)
	{
		//R14 has no AuxHeader section (AutoCAD native files have a 5-record locator)
		if (this.IsR14 && name == DwgSectionDefinition.AuxHeader)
			return;

		//R13-R2000 leave the unknown/ObjFreeSpace locator record empty (0,0),
		//matching AutoCAD native files; the R2004-style content is not valid here
		if (name == DwgSectionDefinition.ObjFreeSpace)
			return;

		this._records[name].Stream = stream;
	}

	public override void WriteFile()
	{
		//1) Seekers for every section except the template
		int offsetAfterObjFree = this.computeSeekers();

		//2) Second header payload: its size locates the Template section,
		//which physically sits after it (like AutoCAD native files)
		byte[] secondHeader = this.buildSecondHeader(offsetAfterObjFree);
		int secondHeaderTotal = _secondHeaderBeginSentinel.Length + secondHeader.Length + _secondHeaderEndSentinel.Length;

		this._records[DwgSectionDefinition.Template].Seeker = offsetAfterObjFree + secondHeaderTotal;

		this.writeFileHeader();

		this.writeRecordStreams();

		//Second header goes after the object map and before the template
		this._stream.Write(_secondHeaderBeginSentinel, 0, _secondHeaderBeginSentinel.Length);
		this._stream.Write(secondHeader, 0, secondHeader.Length);
		this._stream.Write(_secondHeaderEndSentinel, 0, _secondHeaderEndSentinel.Length);

		//Template (MEASUREMENT) is the last section in the file
		this.writeSingleStream(this._records[DwgSectionDefinition.Template]);

		this._stream.Flush();
	}

	/// <summary>
	/// Sets the seeker of every section except the template.
	/// </summary>
	/// <returns>The stream offset right after the ObjFreeSpace record (where the second header begins).</returns>
	private int computeSeekers()
	{
		var currOffset = this.FileHeaderSize;
		foreach (var kv in this._records)
		{
			if (kv.Key == DwgSectionDefinition.Template)
				continue;

			var record = kv.Value;
			record.Seeker = currOffset;
			currOffset += record.Stream == null ? 0 : (int)record.Stream.Length;
		}

		return currOffset;
	}

	private void writeFileHeader()
	{
		System.IO.MemoryStream ms = new System.IO.MemoryStream();

		//0x00	6	“ACXXXX” version string
		IDwgStreamWriter writer = DwgStreamWriterBase.GetStreamWriter(this._version, ms, this._encoding);
		writer.WriteBytes(Encoding.ASCII.GetBytes(this._document.Header.VersionString), 0, 6);
		//The next 7 starting at offset 0x06 are to be six bytes of 0
		//(in R14, 5 0’s and the ACADMAINTVER variable) and a byte of 1.
		//ACADMAINTVER matches the AutoCAD native writers: R14 writes 10, R2000 writes 15.
		byte maintVer = this.IsR14 ? (byte)0x0A : (byte)0x0F;
		writer.WriteBytes(new byte[7] { 0, 0, 0, 0, 0, maintVer, 1 });
		//At 0x0D is a seeker (4 byte long absolute address) for the beginning sentinel of the image data.
		writer.WriteRawLong(this._records[DwgSectionDefinition.Preview].Seeker);

		//0x11-0x12: undocumented version bytes; AutoCAD native R14/R2000 files write 0x21 0xAE
		//(also mirrored by the second header dwg_versions field)
		writer.WriteByte(0x21);
		writer.WriteByte(0xAE);

		//Bytes at 0x13 and 0x14 are a raw short indicating the value of the code page for this drawing file.
		writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(this.getFileCodePage()));
		int nRecords = this.IsR14 ? _nRecordsR14 : _nRecordsR2000;
		writer.WriteBytes(LittleEndianConverter.Instance.GetBytes(nRecords), 0, 4);

		this.writeRecord(writer, this._records[DwgSectionDefinition.Header]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.Classes]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.Handles]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.ObjFreeSpace]);
		this.writeRecord(writer, this._records[DwgSectionDefinition.Template]);
		if (!this.IsR14)
			this.writeRecord(writer, this._records[DwgSectionDefinition.AuxHeader]);

		//CRC
		writer.WriteSpearShift();
		writer.WriteRawShort((short)CRC8StreamHandler.GetCRCValue(0xC0C1, ms.GetBuffer(), 0L, ms.Length));

		//0x95,0xA0,0x4E,0x28,0x99,0x82,0x1A,0xE5,0x5E,0x41,0xE0,0x5F,0x9D,0x3A,0x4D,0x00
		writer.WriteBytes(_endSentinel, 0, _endSentinel.Length);

		this._stream.Write(ms.GetBuffer(), 0, (int)ms.Length);
	}

	private void writeRecord(IDwgStreamWriter writer, DwgSectionLocatorRecord record)
	{
		//Record number (raw byte) | Seeker (raw long) | Size (raw long)
		writer.WriteByte((byte)record.Number.Value);
		if (record.Stream == null)
		{
			//Empty section record (ObjFreeSpace in R13-R2000): AutoCAD writes 0,0
			writer.WriteRawLong(0);
			writer.WriteRawLong(0);
			return;
		}

		writer.WriteRawLong(record.Seeker);
		writer.WriteRawLong(record.Stream.Length);
	}

	private void writeRecordStreams()
	{
		foreach (KeyValuePair<string, DwgSectionLocatorRecord> item in this._records)
		{
			if (item.Key == DwgSectionDefinition.Template)
				continue; //written after the second header

			this.writeSingleStream(item.Value);
		}
	}

	private void writeSingleStream(DwgSectionLocatorRecord record)
	{
		if (record.Stream == null)
			return;

		this._stream.Write(record.Stream.GetBuffer(), 0, (int)record.Stream.Length);
	}

	/// <summary>
	/// Builds the R13-R2000 second header payload (between the begin/end sentinels),
	/// replicating the AutoCAD native layout:
	/// RL size | BL address | TFF version(11) | RC maint | RC zero_one_or_three |
	/// BSx dwg_versions | RS codepage | BS num_sections + sections |
	/// BS num_handles + 14 handle entries | RS CRC(0xC0C1) | RLL junk.
	/// </summary>
	private byte[] buildSecondHeader(long beginAddress)
	{
		BitWriter bits = new BitWriter();

		//RL size: patched after the payload is complete (4 bytes, byte aligned)
		bits.writeRL(0);

		//BL address: absolute address of the begin sentinel
		bits.writeBL((ulong)beginAddress);

		//TFF version string, 11 bytes
		byte[] version = new byte[11];
		byte[] vname = Encoding.ASCII.GetBytes(this._document.Header.VersionString);
		System.Array.Copy(vname, version, System.Math.Min(6, vname.Length));
		foreach (byte b in version)
			bits.writeRC(b);

		//RC maint_rel_version | RC zero_one_or_three
		bits.writeRC(this.IsR14 ? (byte)0x0A : (byte)0x0F);
		bits.writeRC(0x01);

		//BSx dwg_versions: mirrors the undocumented header bytes 0x11-0x12 (0x21 0xAE)
		bits.writeBS(unchecked((short)0xAE21));

		//RS codepage
		bits.writeRS(this.getFileCodePage());

		//BS num_sections + section records
		//0: Header, 1: Classes, 2: Object map, 3: ObjFreeSpace(empty), 4: Template(empty), 5: AuxHeader
		//Sections 3 and 4 are always written empty by AutoCAD (sections[4] is empty even
		//though the template exists at the end of the file).
		List<KeyValuePair<string, DwgSectionLocatorRecord>> sections = new List<KeyValuePair<string, DwgSectionLocatorRecord>>
		{
			new KeyValuePair<string, DwgSectionLocatorRecord>(DwgSectionDefinition.Header, this._records[DwgSectionDefinition.Header]),
			new KeyValuePair<string, DwgSectionLocatorRecord>(DwgSectionDefinition.Classes, this._records[DwgSectionDefinition.Classes]),
			new KeyValuePair<string, DwgSectionLocatorRecord>(DwgSectionDefinition.Handles, this._records[DwgSectionDefinition.Handles]),
			new KeyValuePair<string, DwgSectionLocatorRecord>(DwgSectionDefinition.ObjFreeSpace, this._records[DwgSectionDefinition.ObjFreeSpace]),
			new KeyValuePair<string, DwgSectionLocatorRecord>(DwgSectionDefinition.Template, this._records[DwgSectionDefinition.Template]),
		};
		if (!this.IsR14)
			sections.Add(new KeyValuePair<string, DwgSectionLocatorRecord>(DwgSectionDefinition.AuxHeader, this._records[DwgSectionDefinition.AuxHeader]));

		bits.writeBS((short)sections.Count);
		foreach (KeyValuePair<string, DwgSectionLocatorRecord> kvp in sections)
		{
			DwgSectionLocatorRecord record = kvp.Value;
			bits.writeRC((byte)record.Number.Value);

			//ObjFreeSpace and Template entries are always empty in the second header
			bool emptyEntry = kvp.Key == DwgSectionDefinition.ObjFreeSpace
				|| kvp.Key == DwgSectionDefinition.Template
				|| record.Stream == null;

			if (emptyEntry)
			{
				bits.writeBL(0);
				bits.writeBL(0);
			}
			else
			{
				bits.writeBL((ulong)record.Seeker);
				bits.writeBL((ulong)record.Stream.Length);
			}
		}

		//BS num_handles + 14 control handles: RC num_hdl | RC nr | RC[] big-endian value
		List<ulong> handles = new List<ulong>
		{
			this._document.Header.HandleSeed,               //0: HANDSEED
			this._document.BlockRecords.Handle,             //1: BLOCK_CONTROL_OBJECT
			this._document.Layers.Handle,                   //2: LAYER_CONTROL_OBJECT
			this._document.TextStyles.Handle,               //3: STYLE_CONTROL_OBJECT
			this._document.LineTypes.Handle,                //4: LTYPE_CONTROL_OBJECT
			this._document.Views.Handle,                    //5: VIEW_CONTROL_OBJECT
			this._document.UCSs.Handle,                     //6: UCS_CONTROL_OBJECT
			this._document.VPorts.Handle,                   //7: VPORT_CONTROL_OBJECT
			this._document.AppIds.Handle,                   //8: APPID_CONTROL_OBJECT
			this._document.DimensionStyles.Handle,          //9: DIMSTYLE_CONTROL_OBJECT
			this._document.VEntityControl?.Handle ?? 0,     //10: VX_CONTROL_OBJECT
			this._document.RootDictionary.Handle,           //11: DICTIONARY_NAMED_OBJECT
			this._document.MLineStyles.Handle,              //12: DICTIONARY_ACAD_MLINESTYLE
			this._document.Groups.Handle,                   //13: DICTIONARY_ACAD_GROUP
		};

		bits.writeBS((short)handles.Count);
		for (int i = 0; i < handles.Count; i++)
		{
			ulong value = handles[i];
			int nBytes = 0;
			ulong v = value;
			do
			{
				nBytes++;
				v >>= 8;
			} while (v != 0);

			bits.writeRC((byte)nBytes);
			bits.writeRC((byte)i);
			for (int shift = (nBytes - 1) * 8; shift >= 0; shift -= 8)
				bits.writeRC((byte)((value >> shift) & 0xFF));
		}

		//Patch the RL size field (first 4 bytes, byte aligned):
		//total payload length = data bits + CRC(2) + junk(8), padded to whole bytes
		int totalBytes = (bits.TotalBits + 2 * 8 + 8 * 8 + 7) / 8;
		bits.patchUInt32(0, (uint)totalBytes);

		//RS CRC (seed 0xC0C1) over the complete bytes preceding the CRC field
		//(includes the size field, as in the final payload layout)
		int crcBytes = bits.IsByteAligned ? bits.ByteCount : bits.ByteCount - 1;
		ushort crc = CRC8StreamHandler.GetCRCValue(0xC0C1, bits.ToArray(), 0, crcBytes);
		bits.writeRS(crc);

		//RLL junk (8 bytes, ignored by readers)
		for (int i = 0; i < 8; i++)
			bits.writeRC(0);

		bits.padToByte();

		byte[] payload = bits.ToArray();
		return payload;
	}

	/// <summary>
	/// Minimal bit-level writer for DWG bitcoded streams (MSB first within each byte).
	/// </summary>
	private class BitWriter
	{
		private List<byte> _buffer = new List<byte>();
		private int _bit;

		public int ByteCount { get { return this._buffer.Count; } }

		public bool IsByteAligned { get { return this._bit == 0; } }

		public int TotalBits { get { return this._buffer.Count * 8 - this._bit; } }

		/// <summary>Patches a raw little-endian uint32 at a byte-aligned offset.</summary>
		public void patchUInt32(int byteOffset, uint value)
		{
			this._buffer[byteOffset] = (byte)(value & 0xFF);
			this._buffer[byteOffset + 1] = (byte)((value >> 8) & 0xFF);
			this._buffer[byteOffset + 2] = (byte)((value >> 16) & 0xFF);
			this._buffer[byteOffset + 3] = (byte)((value >> 24) & 0xFF);
		}

		public byte[] ToArray()
		{
			return this._buffer.ToArray();
		}

		public void padToByte()
		{
			if (this._bit > 0)
				this.flushByte();
		}

		private void writeBit(int bit)
		{
			if (this._bit == 0)
				this._buffer.Add(0);

			if (bit != 0)
				this._buffer[this._buffer.Count - 1] |= (byte)(0x80 >> this._bit);

			this._bit++;
			if (this._bit == 8)
				this.flushByte();
		}

		private void flushByte()
		{
			this._bit = 0;
		}

		public void writeRC(byte value)
		{
			for (int i = 7; i >= 0; i--)
				this.writeBit((value >> i) & 1);
		}

		public void writeRS(ushort value)
		{
			this.writeRC((byte)(value & 0xFF));
			this.writeRC((byte)((value >> 8) & 0xFF));
		}

		public void writeRL(uint value)
		{
			this.writeRS((ushort)(value & 0xFFFF));
			this.writeRS((ushort)((value >> 16) & 0xFFFF));
		}

		/// <summary>Bitshort: 2-bit code; 0 = raw short, 1 = raw byte, 2 = zero.</summary>
		public void writeBS(short value)
		{
			if (value == 0)
			{
				this.writeBit(1);
				this.writeBit(0);
				return;
			}

			if (value > 0 && value <= 255)
			{
				this.writeBit(0);
				this.writeBit(1);
				this.writeRC((byte)value);
				return;
			}

			this.writeBit(0);
			this.writeBit(0);
			this.writeRS((ushort)value);
		}

		/// <summary>Bitlong: 2-bit code; 0 = raw long, 1 = raw byte, 2 = zero.</summary>
		public void writeBL(ulong value)
		{
			if (value == 0)
			{
				this.writeBit(1);
				this.writeBit(0);
				return;
			}

			if (value <= 255)
			{
				this.writeBit(0);
				this.writeBit(1);
				this.writeRC((byte)value);
				return;
			}

			this.writeBit(0);
			this.writeBit(0);
			this.writeRL((uint)value);
		}
	}
}
