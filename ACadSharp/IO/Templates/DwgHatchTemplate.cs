using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgHatchTemplate : DwgEntityTemplate
	{
		public class DwgBoundaryPathTemplate : ICadObjectBuilder
		{
			public HatchBoundaryPath Path { get; set; } = new HatchBoundaryPath();
			public List<ulong> Handles { get; set; } = new List<ulong>();

			public DwgBoundaryPathTemplate() { }

			public void Build(DwgDocumentBuilder builder)
			{
				foreach (var handle in Handles)
				{
					var edge = builder.GetCadObject(handle);
					throw new NotImplementedException();
				}
			}
		}

		private List<DwgBoundaryPathTemplate> _pathTempaltes = new List<DwgBoundaryPathTemplate>();

		public DwgHatchTemplate(Hatch hatch) : base(hatch) { }

		/// <summary>
		/// Add the path to the hatch and the templates list.
		/// </summary>
		/// <param name="template"></param>
		public void AddPath(DwgBoundaryPathTemplate template)
		{
			(this.CadObject as Hatch).Paths.Add(template.Path);
			this._pathTempaltes.Add(template);
		}

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (DwgBoundaryPathTemplate t in _pathTempaltes)
			{
				t.Build(builder);
			}
		}
	}
}
