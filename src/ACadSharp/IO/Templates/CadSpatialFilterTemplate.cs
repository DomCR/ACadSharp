using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadSpatialFilterTemplate : CadNonGraphicalObjectTemplate
	{
		public bool HasFrontPlane { get; set; } = false;

		public bool InsertTransformRead { get; set; } = false;

		public CadSpatialFilterTemplate() : base(new SpatialFilter())
		{
		}

		public CadSpatialFilterTemplate(SpatialFilter obj) : base(obj)
		{
		}
	}
}