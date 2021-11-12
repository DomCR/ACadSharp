namespace ACadSharp.IO.DWG
{
	internal abstract class DwgSectionReader
	{
		/// <summary>
		/// R13-R14 Only
		/// </summary>
		protected bool R13_14Only;
		/// <summary>
		/// R13-R15 Only
		/// </summary>
		protected bool R13_15Only;
		/// <summary>
		/// R2000+ Only
		/// </summary>
		protected bool R2000Plus;
		/// <summary>
		/// Pre-2004 Only
		/// </summary>
		protected bool R2004Pre;
		/// <summary>
		/// R2004+
		/// </summary>
		protected bool R2004Plus;
		/// <summary>
		/// +R2007 Only
		/// </summary>
		protected bool R2007Plus;
		/// <summary>
		/// R2010+ Only
		/// </summary>
		protected bool R2010Plus;
		/// <summary>
		/// R2013+
		/// </summary>
		protected bool R2013Plus;
		/// <summary>
		/// R2018+
		/// </summary>
		protected bool R2018Plus;

		protected readonly ACadVersion _version;

		public DwgSectionReader(ACadVersion version)
		{
			_version = version;

			R13_14Only = version == ACadVersion.AC1014 || version == ACadVersion.AC1012;
			R13_15Only = version >= ACadVersion.AC1012 && version <= ACadVersion.AC1015;
			R2000Plus = version >= ACadVersion.AC1015;
			R2004Pre = version < ACadVersion.AC1018;
			R2004Plus = version >= ACadVersion.AC1018;
			R2007Plus = version >= ACadVersion.AC1021;
			R2010Plus = version >= ACadVersion.AC1024;
			R2013Plus = version >= ACadVersion.AC1027;
			R2018Plus = version >= ACadVersion.AC1032;
		}
	}
}
