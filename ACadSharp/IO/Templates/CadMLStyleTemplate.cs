using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadMLStyleTemplate : CadTemplate<MLStyle>
	{
		public class ElementTemplate : ICadObjectTemplate
		{
			public ulong? LinetypeHandle { get; set; }
			public int? LinetypeIndex { get; set; }

			public MLStyle.Element Element { get; set; }

			public ElementTemplate(MLStyle.Element element)
			{
				this.Element = element;
			}

			public void Build(CadDocumentBuilder builder)
			{
				if (this.LinetypeHandle.HasValue)
				{
					this.Element.LineType = builder.GetCadObject<LineType>(this.LinetypeHandle.Value);
				}
				else if (this.LinetypeIndex.HasValue)
				{
					if (this.LinetypeIndex == short.MaxValue)
					{
						this.Element.LineType = builder.LineTypes["ByLayer"];
					}
					else if (this.LinetypeIndex == (short.MaxValue - 1))
					{
						this.Element.LineType = builder.LineTypes["ByBlock"];
					}
					else
					{
						try
						{
							//It can be assigned but is not checked
							this.Element.LineType = builder.LineTypes.ElementAt(this.LinetypeIndex.Value).Value;
						}
						catch (System.Exception)
						{
							//TODO: Implement get linetype by index
							builder.Notify(new NotificationEventArgs($"Linetype not assigned, index {LinetypeIndex}"));
						}
					}
				}
			}
		}

		public List<ElementTemplate> ElementTemplates { get; set; } = new List<ElementTemplate>();

		public CadMLStyleTemplate(MLStyle mlStyle) : base(mlStyle) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var item in this.ElementTemplates)
			{
				item.Build(builder);
			}
		}
	}
}
