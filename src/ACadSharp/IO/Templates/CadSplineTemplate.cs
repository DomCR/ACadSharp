using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadSplineTemplate : CadEntityTemplate<Spline>
	{
		public CadSplineTemplate() : base(new Spline()) { }

		public CadSplineTemplate(Spline entity) : base(entity) { }
	}
}
