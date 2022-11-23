namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeader21 : DwgFileHeaderAC18
	{
		public Dwg21CompressedMetadata CompressedMetadata { get; set; }
		
		public DwgFileHeader21() : base() { }

		public DwgFileHeader21(ACadVersion version) : base(version) { }
	}
}
