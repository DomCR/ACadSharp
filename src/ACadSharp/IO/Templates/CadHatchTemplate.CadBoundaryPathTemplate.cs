using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadHatchTemplate
	{
		public class CadBoundaryPathTemplate : ICadTemplate
		{
			public Hatch.BoundaryPath Path { get; set; }

			public HashSet<ulong> Handles { get; set; } = new();

			public CadBoundaryPathTemplate() { }

			public void Build(CadDocumentBuilder builder)
			{
				if (this.Path == null)
				{
					return;
				}

				foreach (var handle in this.Handles)
				{
					if (builder.TryGetCadObject(handle, out Entity entity))
					{
						this.Path.Entities.Add(entity);
					}
				}
			}
		}
	}
}
