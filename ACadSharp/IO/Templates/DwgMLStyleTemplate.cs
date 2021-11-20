using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgMLStyleTemplate : DwgTemplate<MLStyle>
	{
		public class ElementTemplate : ICadObjectBuilder
		{
			public ulong? LinetypeHandle { get; set; }
			public int? LinetypeIndex { get; set; }

			public MLStyle.Element Element { get; set; }

			public ElementTemplate(MLStyle.Element element)
			{
				this.Element = element;
			}

			public void Build(DwgDocumentBuilder builder)
			{
				if (this.LinetypeHandle.HasValue)
				{
					this.Element.LineType = builder.GetCadObject<LineType>(this.LinetypeHandle.Value);
				}
				else if (this.LinetypeIndex.HasValue)
				{
					//TODO: Implement get linetype by index
					//index = 32766??	//by block?
					//index = 32767??	//by layer??
					builder.NotificationHandler?.Invoke(
						this.Element,
						new NotificationEventArgs($"Linetype not assigned, index {LinetypeIndex}"));;
				}
			}
		}

		public List<ElementTemplate> ElementTemplates { get; set; } = new List<ElementTemplate>();

		public DwgMLStyleTemplate(MLStyle mlStyle) : base(mlStyle) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var item in ElementTemplates)
			{
				item.Build(builder);
			}
		}
	}
}
