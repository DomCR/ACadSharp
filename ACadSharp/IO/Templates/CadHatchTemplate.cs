using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadHatchTemplate : CadEntityTemplate
	{
		public string HatchPatternName;

		public List<CadBoundaryPathTemplate> PathTempaltes = new List<CadBoundaryPathTemplate>();

		public CadHatchTemplate(Hatch hatch) : base(hatch) { }

		public override bool AddName(int dxfcode, string name)
		{
			bool value = base.AddName(dxfcode, name);

			switch (dxfcode)
			{
				case 2:
					this.HatchPatternName = name;
					value = true;
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (CadBoundaryPathTemplate t in PathTempaltes)
			{
				(this.CadObject as Hatch).Paths.Add(t.Path);
				t.Build(builder);
			}
		}
	}
}
