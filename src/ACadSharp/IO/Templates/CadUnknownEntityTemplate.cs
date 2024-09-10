using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadUnknownNonGraphicalObjectTemplate : CadNonGraphicalObjectTemplate
	{
		public CadUnknownNonGraphicalObjectTemplate(UnknownNonGraphicalObject obj) : base(obj) { }
	}

	internal class CadUnknownEntityTemplate : CadEntityTemplate
	{
		public CadUnknownEntityTemplate(UnknownEntity entity) : base(entity) { }
	}
}
