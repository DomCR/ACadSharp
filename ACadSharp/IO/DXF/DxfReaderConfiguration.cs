namespace ACadSharp.IO
{
	public class DxfReaderConfiguration : CadReaderConfiguration
	{
		/// <summary>
		/// Clears the cache after the reading
		/// </summary>
		public bool ClearChache { get; set; } = true;
	}
}
