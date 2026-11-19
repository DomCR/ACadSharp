namespace ACadSharp.IO
{
	public class DwgWriterConfiguration : CadWriterConfiguration
	{
		/// <summary>
		/// Preserve ASM binary payloads inline when writing DWG 2004, 2007 or 2010.
		/// Defaults to false, which writes an empty payload with a warning for these targets.
		/// </summary>
		/// <remarks>
		/// This preserves the original bytes without converting the modeling-kernel version.
		/// Some ASM payloads can be opened in these DWG versions, while others require recovery.
		/// Enable only for payloads whose compatibility has been verified in the target application.
		/// DWG 2000 and earlier retain the warning/empty-payload fallback regardless of this setting.
		/// </remarks>
		public bool WriteLegacyAsmData { get; set; }
	}
}
