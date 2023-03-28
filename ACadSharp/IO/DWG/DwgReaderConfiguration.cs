namespace ACadSharp.IO
{
	public class DwgReaderConfiguration : CadReaderConfiguration
	{
		/// <summary>
		/// Use the Standard Cycling Redundancy Check to verify the integrity of the file.
		/// </summary>
		/// <remarks>
		/// DWG file format uses a modification of a standard Cyclic Redundancy Check as an error detecting mechanism, 
		/// if this flag is enabled the reader will perform this verification to detect any possible error, but it will greatly increase the reading time.
		/// </remarks>
		public bool CrcCheck { get; set; } = false;
	}
}
