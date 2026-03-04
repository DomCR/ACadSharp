using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates;

internal class CadDimensionAssociationTemplate : CadTemplate<DimensionAssociation>
{
	public ulong? DimensionHandle { get; set; }

	public OsnapPointRefTemplate FirstPointRef { get; set; }

	public OsnapPointRefTemplate FourthPointRef { get; set; }

	public OsnapPointRefTemplate SecondPointRef { get; set; }

	public OsnapPointRefTemplate ThirdPointRef { get; set; }

	public CadDimensionAssociationTemplate() : base(new())
	{
	}

	public CadDimensionAssociationTemplate(DimensionAssociation obj) : base(obj)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (builder.TryGetCadObject<Dimension>(this.DimensionHandle, out var dimension))
		{
			this.CadObject.Dimension = dimension;
		}

		if (this.FirstPointRef != null)
		{
			this.CadObject.FirstPointRef = this.FirstPointRef.OsnapPointRef;
			this.FirstPointRef.Build(builder);
		}

		if (this.SecondPointRef != null)
		{
			this.CadObject.SecondPointRef = this.SecondPointRef.OsnapPointRef;
			this.SecondPointRef.Build(builder);
		}

		if (this.ThirdPointRef != null)
		{
			this.CadObject.ThirdPointRef = this.ThirdPointRef.OsnapPointRef;
			this.ThirdPointRef.Build(builder);
		}

		if (this.FourthPointRef != null)
		{
			this.CadObject.FourthPointRef = this.FourthPointRef.OsnapPointRef;
			this.FourthPointRef.Build(builder);
		}
	}

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