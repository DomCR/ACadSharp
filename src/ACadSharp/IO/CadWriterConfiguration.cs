using ACadSharp.Classes;

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

		/// <summary>
		/// Will not ignore the <see cref="ACadSharp.XData.ExtendedData"/> collection in the <see cref="CadObject"/>.
		/// </summary>
		/// <value>
		/// default: true
		/// </value>
		public bool WriteXData { get; set; } = true;

		/// <summary>
		/// Resets the <see cref="DxfClass"/> collection in the <see cref="CadDocument"/> before writing it.
		/// </summary>
		/// <remarks>
		/// Sometimes the files are corrupted by badly formed dxf classes, is recommended to keep this flag set.
		/// </remarks>
		public bool ResetDxfClasses { get; set; } = true;
	}
}
