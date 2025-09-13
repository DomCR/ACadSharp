using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadHatchTemplate : CadEntityTemplate<Hatch>
	{
		public string HatchPatternName { get; set; }

		public List<CadBoundaryPathTemplate> PathTempaltes = new List<CadBoundaryPathTemplate>();

		public CadHatchTemplate() : base(new Hatch()) { }

		public CadHatchTemplate(Hatch hatch) : base(hatch) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (!string.IsNullOrEmpty(this.HatchPatternName))
			{

			}

			foreach (CadBoundaryPathTemplate t in PathTempaltes)
			{
				(this.CadObject as Hatch).Paths.Add(t.Path);
				t.Build(builder);
			}
		}
	}
}
