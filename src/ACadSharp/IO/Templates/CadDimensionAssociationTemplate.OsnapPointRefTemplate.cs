using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal partial class CadDimensionAssociationTemplate
{
	public class OsnapPointRefTemplate : ICadTemplate
	{
		public ulong? ObjectHandle { get; set; }
		public List<ulong> GeometryHandles { get; } = new();
		public List<ulong> IntersectionHandles { get; } = new();
		public OsnapPointRefTemplate LastPoint { get; set; }

		public DimensionAssociation.OsnapPointRef OsnapPointRef { get; set; }

		public OsnapPointRefTemplate(DimensionAssociation.OsnapPointRef pointRef)
		{
			OsnapPointRef = pointRef;
		}

		public void Build(CadDocumentBuilder builder)
		{
			if (builder.TryGetCadObject<CadObject>(this.ObjectHandle, out var obj))
			{
				OsnapPointRef.Geometry = obj;
			}
			foreach (ulong handle in this.GeometryHandles)
				if (builder.TryGetCadObject<CadObject>(handle, out var geometry)) this.OsnapPointRef.GeometryPath.Add(geometry);
			foreach (ulong handle in this.IntersectionHandles)
				if (builder.TryGetCadObject<CadObject>(handle, out var geometry)) this.OsnapPointRef.IntersectionGeometry.Add(geometry);
			this.LastPoint?.Build(builder);
			this.OsnapPointRef.LastPointRef = this.LastPoint?.OsnapPointRef;
		}
	}
}