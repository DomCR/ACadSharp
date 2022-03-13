using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadHatchTemplate : CadEntityTemplate
	{
		public class CadBoundaryPathTemplate : ICadObjectBuilder
		{
			public Hatch.BoundaryPath Path { get; set; } = new Hatch.BoundaryPath();

			public List<ulong> Handles { get; set; } = new List<ulong>();

			public CadBoundaryPathTemplate() { }

			public void Build(CadDocumentBuilder builder)
			{
				foreach (var handle in Handles)
				{
					if (builder.TryGetCadObject(handle, out Entity entity))
					{
						Path.Entities.Add(entity);
					}
				}
			}
		}

		public List<CadBoundaryPathTemplate> PathTempaltes = new List<CadBoundaryPathTemplate>();

		public CadHatchTemplate(Hatch hatch) : base(hatch) { }

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
