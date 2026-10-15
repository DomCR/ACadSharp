using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadHatchTemplate
	{
		public class CadBoundaryPathTemplate : ICadTemplate
		{
			public Hatch.BoundaryPath Path { get; set; } = new();

			// A list, not a set: AutoCAD cites the same boundary object more than once in a single loop,
			// and the duplicate is part of the association (AcDbHatch::getAssocObjIdsAt returns an
			// AcDbObjectIdArray). A set dropped it on read, so the next save lost the link.
			public List<ulong> Handles { get; set; } = new();

			public CadBoundaryPathTemplate() { }

			public void Build(CadDocumentBuilder builder)
			{
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
