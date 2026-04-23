using ACadSharp.Tables;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate
{
	internal class CellContentFormatTemplate : ICadTemplate
	{
		public ContentFormat Format { get; }

		public ulong? TextStyleHandle { get; set; }

		public string TextStyleName { get; set; }

		public CellContentFormatTemplate(ContentFormat format)
		{
			this.Format = format;
		}

		public void Build(CadDocumentBuilder builder)
		{
			if (builder.TryGetCadObject(this.TextStyleHandle, out TextStyle textStyle) 
				|| builder.TryGetTableEntry(this.TextStyleName, out textStyle))
			{
				this.Format.TextStyle = textStyle;
			}
			else
			{
				builder.Notify($"{typeof(TextStyle).FullName} table reference with handle: {this.TextStyleName} | name: {this.TextStyleName} not found for {this.Format.GetType().FullName}", NotificationType.Warning);
			}
		}
	}
}