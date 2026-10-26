using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates;

internal partial class CadDimensionAssociationTemplate : CadTemplate<DimensionAssociation>
{
	public ulong? DimensionHandle { get; set; }

	public OsnapPointRefTemplate FirstPointRef { get; set; }

	public OsnapPointRefTemplate FourthPointRef { get; set; }

	public OsnapPointRefTemplate SecondPointRef { get; set; }

	public OsnapPointRefTemplate ThirdPointRef { get; set; }

	/// <summary>
	/// The reference the DXF reader read last, to chain the next one when it has 75 = 1.
	/// </summary>
	public OsnapPointRefTemplate LastReadPointRef { get; set; }

	public CadDimensionAssociationTemplate() : base(new())
	{
	}

	public CadDimensionAssociationTemplate(DimensionAssociation obj) : base(obj)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		//A dimension or a leader (LEADER, MULTILEADER): see DimensionAssociation.AssociatedEntity
		if (builder.TryGetCadObject<Entity>(this.DimensionHandle, out var entity))
		{
			this.CadObject.AssociatedEntity = entity;
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
}