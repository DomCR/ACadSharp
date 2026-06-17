using ACadSharp.Entities;
using ACadSharp.Objects;
using CSMath;

namespace ACadSharp.IO.Templates;

internal class CadMeshTemplate : CadEntityTemplate<Mesh>
{
	public bool SubclassMarker { get; set; } = false;

	public CadMeshTemplate() { }

	public CadMeshTemplate(Mesh mesh) : base(mesh) { }

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);
	}
}
