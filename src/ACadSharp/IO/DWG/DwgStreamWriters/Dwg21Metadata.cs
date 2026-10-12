// AC1021 (R2007) compressed metadata (0x110 = 34 × u64), ODA spec §5.2.
// Field order follows cadcodec (Rust reference implementation, dwg21_metadata.rs),
// which matches ACadSharp's reader (DwgReader.readFileHeaderAC21).
using CSUtilities.IO;
using System;
using System.IO;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class Dwg21Metadata
{
	public ulong HeaderSize { get; set; } = 0x70;
	public ulong FileSize { get; set; }
	public ulong PagesMapCrcCompressed { get; set; }
	public ulong PagesMapCorrectionFactor { get; set; }
	public ulong PagesMapCrcSeed { get; set; }
	public ulong Map2Offset { get; set; }
	public ulong Map2Id { get; set; }
	public ulong PagesMapOffset { get; set; }
	public ulong PagesMapId { get; set; }
	public ulong Header2offset { get; set; }
	public ulong PagesMapSizeCompressed { get; set; }
	public ulong PagesMapSizeUncompressed { get; set; }
	public ulong PagesAmount { get; set; }
	public ulong PagesMaxId { get; set; }
	public ulong Unknow0x20 { get; set; } = 0x20;
	public ulong Unknow0x40 { get; set; } = 0x40;
	public ulong PagesMapCrcUncompressed { get; set; }
	public ulong Unknown0xF800 { get; set; } = 0xF800;
	public ulong Unknown4 { get; set; } = 4;
	public ulong Unknown1 { get; set; } = 1;
	public ulong SectionsAmount { get; set; }
	public ulong SectionsMapCrcUncompressed { get; set; }
	public ulong SectionsMapSizeCompressed { get; set; }
	public ulong SectionsMap2Id { get; set; }
	public ulong SectionsMapId { get; set; }
	public ulong SectionsMapSizeUncompressed { get; set; }
	public ulong SectionsMapCrcCompressed { get; set; }
	public ulong SectionsMapCorrectionFactor { get; set; }
	public ulong SectionsMapCrcSeed { get; set; }
	public ulong StreamVersion { get; set; } = 0x60100;
	public ulong CrcSeed { get; set; }
	public ulong CrcSeedEncoded { get; set; }
	public ulong RandomSeed { get; set; }
	public ulong HeaderCrc64 { get; set; }

	/// <summary>Serialize all 34 fields as little-endian u64 into a 0x110-byte buffer.</summary>
	public byte[] ToBytes()
	{
		using MemoryStream stream = new MemoryStream(0x110);
		StreamIO writer = new StreamIO(stream);

		writer.Write(this.HeaderSize);
		writer.Write(this.FileSize);
		writer.Write(this.PagesMapCrcCompressed);
		writer.Write(this.PagesMapCorrectionFactor);
		writer.Write(this.PagesMapCrcSeed);
		writer.Write(this.Map2Offset);
		writer.Write(this.Map2Id);
		writer.Write(this.PagesMapOffset);
		writer.Write(this.PagesMapId);
		writer.Write(this.Header2offset);
		writer.Write(this.PagesMapSizeCompressed);
		writer.Write(this.PagesMapSizeUncompressed);
		writer.Write(this.PagesAmount);
		writer.Write(this.PagesMaxId);
		writer.Write(this.Unknow0x20);
		writer.Write(this.Unknow0x40);
		writer.Write(this.PagesMapCrcUncompressed);
		writer.Write(this.Unknown0xF800);
		writer.Write(this.Unknown4);
		writer.Write(this.Unknown1);
		writer.Write(this.SectionsAmount);
		writer.Write(this.SectionsMapCrcUncompressed);
		writer.Write(this.SectionsMapSizeCompressed);
		writer.Write(this.SectionsMap2Id);
		writer.Write(this.SectionsMapId);
		writer.Write(this.SectionsMapSizeUncompressed);
		writer.Write(this.SectionsMapCrcCompressed);
		writer.Write(this.SectionsMapCorrectionFactor);
		writer.Write(this.SectionsMapCrcSeed);
		writer.Write(this.StreamVersion);
		writer.Write(this.CrcSeed);
		writer.Write(this.CrcSeedEncoded);
		writer.Write(this.RandomSeed);
		writer.Write(this.HeaderCrc64);

		return stream.ToArray();
	}
}
