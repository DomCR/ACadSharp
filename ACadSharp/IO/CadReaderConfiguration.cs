namespace ACadSharp.IO
{
	public abstract class CadReaderConfiguration
	{
		/// <summary>
		/// The reader will try to continue when an exception is thrown
		/// </summary>
		/// <remarks>
		/// The result file may be incomplete or with some objects missing due the error
		/// </remarks>
		public bool Failsafe { get; set; } = true;
	}
}
