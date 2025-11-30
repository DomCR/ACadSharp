using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="PdfUnderlayDefinition"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectPdfDefinition"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.UnderlayDefinition"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectPdfDefinition)]
	[DxfSubClass(DxfSubclassMarker.UnderlayDefinition)]
	public class PdfUnderlayDefinition : UnderlayDefinition
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectPdfDefinition;

		/// <summary>
		/// Gets or sets the PDF page to show.
		/// </summary>
		[DxfCodeValue(2)]
		public string Page
		{
			get { return this._page; }
			set { this._page = string.IsNullOrEmpty(value) ? string.Empty : value; }
		}

		private string _page;

		/// <inheritdoc/>
		public PdfUnderlayDefinition() : base()
		{
		}
	}
}
