namespace ACadSharp.IO.DWG
{
	internal class Dwg21CompressedMetadata
	{
		public ulong HeaderSize { get; set; } = 0x70;
		public ulong FileSize { get; set; }
		public ulong PagesMapCrcCompressed { get; set; }
		public ulong PagesMapCorrectionFactor { get; set; }
		public ulong PagesMapCrcSeed { get; set; }
		public ulong Map2Offset { get; set; }
		public ulong Map2Id { get; set; }
		public ulong PagesMapOffset { get; set; }
		public ulong Header2offset { get; set; }
		public ulong PagesMapSizeCompressed { get; set; }
		public ulong PagesMapSizeUncompressed { get; set; }
		public ulong PagesAmount { get; set; }
		public ulong PagesMaxId { get; set; }
		public ulong SectionsMap2Id { get; set; }
		public ulong PagesMapId { get; set; }
		public ulong Unknow0x20 { get; set; } = 32;
		public ulong Unknow0x40 { get; set; } = 64;
		public ulong PagesMapCrcUncompressed { get; set; }
		public ulong Unknown0xF800 { get; set; } = 0xF800;
		public ulong Unknown4 { get; set; } = 4;
		public ulong Unknown1 { get; set; } = 1;
		public ulong SectionsAmount { get; set; }
		public ulong SectionsMapCrcUncompressed { get; set; }
		public ulong SectionsMapSizeCompressed { get; set; }
		public ulong SectionsMapId { get; set; }
		public ulong SectionsMapSizeUncompressed { get; set; }
		public ulong SectionsMapCrcCompressed { get; set; }
		public ulong SectionsMapCorrectionFactor { get; set; }
		public ulong SectionsMapCrcSeed { get; set; }
		public ulong StreamVersion { get; set; } = 393472;
		public ulong CrcSeed { get; set; }
		public ulong CrcSeedEncoded { get; set; }
		public ulong RandomSeed { get; set; }
		public ulong HeaderCRC64 { get; set; }
	}
}
