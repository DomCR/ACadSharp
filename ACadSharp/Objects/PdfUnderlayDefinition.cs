namespace ACadSharp.Objects
{
	public class PdfUnderlayDefinition : UnderlayDefinition
	{
		/// <inheritdoc/>
		public override string ObjectName => base.ObjectName;

		/// <summary>
		/// Gets or sets the PDF page to show.
		/// </summary>
		public string Page
		{
			get { return this._page; }
			set { this._page = string.IsNullOrEmpty(value) ? string.Empty : value; }
		}

		private string _page;
	}
}
