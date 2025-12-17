using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadHatchTemplate : CadEntityTemplate<Hatch>
	{
		public string HatchPatternName { get; set; }

		public List<CadBoundaryPathTemplate> PathTemplates = new List<CadBoundaryPathTemplate>();

		public CadHatchTemplate() : base(new Hatch()) { }

		public CadHatchTemplate(Hatch hatch) : base(hatch) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (CadBoundaryPathTemplate t in PathTemplates)
			{
				(this.CadObject as Hatch).Paths.Add(t.Path);
				t.Build(builder);
			}
		}
	}
}
