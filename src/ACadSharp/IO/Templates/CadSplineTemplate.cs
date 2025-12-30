using ACadSharp.Entities;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadSplineTemplate : CadEntityTemplate<Spline>
	{
		public CadSplineTemplate() : base(new Spline()) { }

		public CadSplineTemplate(Spline entity) : base(entity) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.CadObject.ControlPoints.Any())
			{
				if (this.CadObject.ControlPoints.First()
					.Equals(this.CadObject.ControlPoints.Last()))
				{
					this.CadObject.IsPeriodic = false;
				}
			}
		}
	}
}
