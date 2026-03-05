using ACadSharp.Objects;

namespace ACadSharp.IO.Templates;

internal partial class CadDimensionAssociationTemplate
{
	public class OsnapPointRefTemplate : ICadTemplate
	{
		public ulong? ObjectHandle { get; set; }

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
		}
	}
}