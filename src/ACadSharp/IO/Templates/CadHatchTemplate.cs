using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadHatchTemplate : CadEntityTemplate<Hatch>
	{
		public string HatchPatternName { get; set; }

		public List<CadBoundaryPathTemplate> PathTemplates = new List<CadBoundaryPathTemplate>();

		// [PATCH] The reader invokes the hatch delegate once per DXF code pair, so the
		// seed-point sequence state (98 count, then the 10/20 pairs) must live in the
		// template — locals in readHatch were re-initialised on every call, so the 98
		// count was lost and the seed point was read as the hatch origin.
		public int SeedPointsLeft = 0;

		public XY SeedPoint = new XY();

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
