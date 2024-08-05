namespace ACadSharp.IO
{
	/// <summary>
	/// Configuration for the <see cref="CadWriterBase{T}"/> class.
	/// </summary>
	public class CadWriterConfiguration
	{
		/// <summary>
		/// The writer will close the stream once the operation is completed.
		/// </summary>
		/// <value>
		/// default: true
		/// </value>
		public bool CloseStream { get; set; } = true;

		/// <summary>
		/// Will not ignore the <see cref="ACadSharp.Objects.XRecord"/> objects in the document.
		/// </summary>
		/// <remarks>
		/// Due the complexity of XRecords, if this flag is set to true, it may cause a corruption of the file if the records have been modified manually.
		/// </remarks>
		/// <value>
		/// default: false
		/// </value>
		public bool WriteXRecords { get; set; } = false;
	}
}
