using ACadSharp.Entities.Mechanical;

namespace ACadSharp.IO.Templates;

internal abstract class CadMechanicalEntityTemplate<T> : CadEntityTemplate<T>
	where T : MechanicalEntity, new()
{
	public ulong? BOMStandardDINHandle { get; set; }

	public ulong? StandardDINHandle { get; set; }

	public CadMechanicalEntityTemplate(T entity) : base(entity)
	{
	}
}
