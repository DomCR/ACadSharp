using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadMeshTemplate : CadEntityTemplate<Mesh>
	{
		public bool SubclassMarker { get; set; } = false;

		public CadMeshTemplate() { }

		public CadMeshTemplate(Mesh mesh) : base(mesh) { }
}
}
