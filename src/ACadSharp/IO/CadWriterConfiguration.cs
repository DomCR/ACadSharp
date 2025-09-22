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
		/// Resets the <see cref="DxfClass"/> collection in the <see cref="CadDocument"/> before writing it.
		/// </summary>
		/// <remarks>
		/// Sometimes the files are corrupted by badly formed dxf classes, is recommended to keep this flag set.
		/// </remarks>
		/// <value>
		/// default: true
		/// </value>
		public bool ResetDxfClasses { get; set; } = true;

		/// <summary>
		///  Update the blocks that visualize the dimensions in the model space.
		/// </summary>
		/// <remarks>
		/// When creating or modifying a dimension in a block, it needs to be updated in order to appear in the drawing.
		/// </remarks>
		/// <value>
		/// default: false
		/// </value>
		public bool UpdateDimensionsInBlocks { get; set; } = false;

		/// <summary>
		/// Update the blocks that visualize the dimensions in the blocks.
		/// </summary>
		/// <remarks>
		/// The dimensions in the model space are automatically updated by the drawing software, is not recommended to update them if is not needed.
		/// </remarks>
		/// <value>
		/// default: false
		/// </value>
		public bool UpdateDimensionsInModel { get; set; } = false;

		/// <summary>
		/// The writer will not ignore the <see cref="ACadSharp.XData.ExtendedData"/> collection in the <see cref="CadObject"/>.
		/// </summary>
		/// <value>
		/// default: true
		/// </value>
		public bool WriteXData { get; set; } = true;

		/// <summary>
		/// The writer will not ignore the <see cref="ACadSharp.Objects.XRecord"/> objects in the document.
		/// </summary>
		/// <remarks>
		/// Due the complexity of XRecords, if this flag is set to true, it may cause a corruption of the file.
		/// </remarks>
		/// <value>
		/// default: false
		/// </value>
		public bool WriteXRecords { get; set; } = false;

		/// <summary>
		/// The writer will not ignore the <see cref="ACadSharp.Entities.Shape"/> entities in the document.
		/// </summary>
		/// <remarks>
		/// Shapes can cause corruption for some documents due the lack of support for shx files in this library which cannot validate the correct shape format.
		/// </remarks>
		public bool WriteShapes { get; set; } = false;
	}
}