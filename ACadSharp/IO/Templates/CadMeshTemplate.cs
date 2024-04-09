using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadMeshTemplate : CadEntityTemplate<Mesh>
	{
		public bool SubclassMarker { get; set; } = false;
	}
}
