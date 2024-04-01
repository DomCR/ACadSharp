namespace ACadSharp.IO
{
	public class DwgReaderConfiguration : CadReaderConfiguration
	{
		/// <summary>
		/// Use the Standard Cycling Redundancy Check to verify the integrity of the file, default value is set to false.
		/// </summary>
		/// <remarks>
		/// DWG file format uses a modification of a standard Cyclic Redundancy Check as an error detecting mechanism, 
		/// if this flag is enabled the reader will perform this verification to detect any possible error, but it will greatly increase the reading time.
		/// </remarks>
		public bool CrcCheck { get; set; } = false;

		/// <summary>
		/// The reader will try to read and add to the document all <see cref="ObjectType.UNLISTED"/> that are linked to a <see cref="Classes. DxfClass"/> 
		/// which may be a proxy or an entity that is not yet supported by ACadSharp, default value is set to false.
		/// </summary>
		/// <remarks>
		/// These entities do not contain any geometric information and will be ignored by the writers
		/// </remarks>
		public bool KeepUnknownEntities { get; set; } = false;
	}
}
