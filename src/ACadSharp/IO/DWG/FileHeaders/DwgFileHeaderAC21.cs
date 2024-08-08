namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderAC21 : DwgFileHeaderAC18
	{
		public Dwg21CompressedMetadata CompressedMetadata { get; set; }
		
		public DwgFileHeaderAC21() : base() { }

		public DwgFileHeaderAC21(ACadVersion version) : base(version) { }
	}
}
