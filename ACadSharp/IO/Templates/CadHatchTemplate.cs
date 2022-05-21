using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadHatchTemplate : CadEntityTemplate
	{
		public string HatchPatternName { get; set; }

		public List<CadBoundaryPathTemplate> PathTempaltes = new List<CadBoundaryPathTemplate>();

		public CadHatchTemplate(Hatch hatch) : base(hatch) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			//TODO: Finish the hatch template

			foreach (CadBoundaryPathTemplate t in PathTempaltes)
			{
				(this.CadObject as Hatch).Paths.Add(t.Path);
				t.Build(builder);
			}
		}
	}
}
