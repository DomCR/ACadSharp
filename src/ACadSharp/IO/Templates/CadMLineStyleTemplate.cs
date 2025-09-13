using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadMLineStyleTemplate : CadTemplate<MLineStyle>
	{
		public class ElementTemplate
		{
			public ulong? LineTypeHandle { get; set; }

			public ulong? LineTypeName { get; set; }

			public int? LinetypeIndex { get; set; }

			public MLineStyle.Element Element { get; set; }

			public ElementTemplate(MLineStyle.Element element)
			{
				this.Element = element;
			}

			public void Build(CadDocumentBuilder builder)
			{
				if (builder.TryGetCadObject(this.LineTypeHandle, out LineType lt))
				{
					this.Element.LineType = lt;
				}
				else if (this.LinetypeIndex.HasValue)
				{
					if (this.LinetypeIndex == short.MaxValue)
					{
						if (builder.TryGetTableEntry<LineType>(LineType.ByLayerName, out LineType bylayer))
						{
							this.Element.LineType = bylayer;
						}
					}
					else if (this.LinetypeIndex == (short.MaxValue - 1))
					{
						if (builder.TryGetTableEntry<LineType>(LineType.ByBlockName, out LineType byblock))
						{
							this.Element.LineType = byblock;
						}
					}
					else
					{
						try
						{
							//It can be assigned but is not checked
							this.Element.LineType = builder.LineTypesTable.ElementAt(this.LinetypeIndex.Value);
						}
						catch (System.Exception ex)
						{
							//TODO: Implement get linetype by index
							builder.Notify($"Linetype not assigned, index {LinetypeIndex}", NotificationType.Error, ex);
						}
					}
				}
			}
		}

		public List<ElementTemplate> ElementTemplates { get; set; } = new List<ElementTemplate>();

		public CadMLineStyleTemplate(MLineStyle mlStyle) : base(mlStyle) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (var item in this.ElementTemplates)
			{
				item.Build(builder);
			}
		}
	}
}
