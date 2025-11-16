using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Linq;
using System.Xml.Linq;

namespace ACadSharp.IO.Templates
{
	internal partial class CadMLineStyleTemplate
	{
		public class ElementTemplate
		{
			public MLineStyle.Element Element { get; set; }

			public ulong? LineTypeHandle { get; set; }

			public int? LinetypeIndex { get; set; }

			public string LineTypeName { get; set; }

			public ElementTemplate(MLineStyle.Element element)
			{
				this.Element = element;
			}

			public void Build(CadDocumentBuilder builder)
			{
				LineType lt;
				if (builder.TryGetCadObject(this.LineTypeHandle, out lt))
				{
					this.Element.LineType = lt;
				}
				else if (builder.TryGetTableEntry(this.LineTypeName, out lt))
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
	}
}