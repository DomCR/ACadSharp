using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgHatchTemplate : CadEntityTemplate
	{
		public class DwgBoundaryPathTemplate : ICadObjectBuilder
		{
			public HatchBoundaryPath Path { get; set; } = new HatchBoundaryPath();
			public List<ulong> Handles { get; set; } = new List<ulong>();

			public DwgBoundaryPathTemplate() { }

			public void Build(CadDocumentBuilder builder)
			{
				foreach (var handle in Handles)
				{
					var edge = builder.GetCadObject(handle);

					builder.NotificationHandler?.Invoke(this.Path, new NotificationEventArgs($"Boundary path with handles not implemented"));
				}
			}
		}

		public List<DwgBoundaryPathTemplate> PathTempaltes = new List<DwgBoundaryPathTemplate>();

		public DwgHatchTemplate(Hatch hatch) : base(hatch) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (DwgBoundaryPathTemplate t in PathTempaltes)
			{
				(this.CadObject as Hatch).Paths.Add(t.Path);
				t.Build(builder);
			}
		}
	}
}
