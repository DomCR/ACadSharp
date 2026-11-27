using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal partial class CadDimensionAssociationTemplate
{
	public class OsnapPointRefTemplate : ICadTemplate
	{
		/// <summary>
		/// Handles of the main path, outermost first (DXF 331, one per object).
		/// </summary>
		public List<ulong> GeometryHandles { get; } = new();

		/// <summary>
		/// Handles of the intersection path, outermost first (DXF 332, one per object).
		/// </summary>
		public List<ulong> IntersectionHandles { get; } = new();

		/// <summary>
		/// The reference chained after this one (see <see cref="DimensionAssociation.OsnapPointRef.LastPointRef"/>).
		/// </summary>
		public OsnapPointRefTemplate LastPointRef { get; set; }

		public DimensionAssociation.OsnapPointRef OsnapPointRef { get; set; }

		public OsnapPointRefTemplate(DimensionAssociation.OsnapPointRef pointRef)
		{
			OsnapPointRef = pointRef;
		}

		public void Build(CadDocumentBuilder builder)
		{
			//A handle that does not resolve is dropped: an object missing from the document cannot be written back
			OsnapPointRef.GeometryPath.Clear();
			foreach (ulong handle in this.GeometryHandles)
			{
				if (builder.TryGetCadObject<CadObject>(handle, out var obj))
				{
					OsnapPointRef.GeometryPath.Add(obj);
				}
			}

			OsnapPointRef.IntersectionPath.Clear();
			foreach (ulong handle in this.IntersectionHandles)
			{
				if (builder.TryGetCadObject<CadObject>(handle, out var obj))
				{
					OsnapPointRef.IntersectionPath.Add(obj);
				}
			}

			if (this.LastPointRef != null)
			{
				OsnapPointRef.LastPointRef = this.LastPointRef.OsnapPointRef;
				this.LastPointRef.Build(builder);
			}
		}
	}
}
