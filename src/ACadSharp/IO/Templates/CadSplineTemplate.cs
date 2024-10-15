using ACadSharp.Entities;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadSplineTemplate : CadEntityTemplate<Spline>
	{
		private CSMath.XYZ _currentControlPoint = new CSMath.XYZ();
		private CSMath.XYZ _currentFitPoint = new CSMath.XYZ();

		public CadSplineTemplate() : base(new Spline()) { }

		public CadSplineTemplate(Spline entity) : base(entity) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Spline spline = this.CadObject as Spline;

			if (spline.Knots.Count > 0)
			{
				spline.Flags1 |= SplineFlags1.UseKnotParameter;
			}

			if (spline.FitPoints.Count > 0)
			{
				spline.Flags1 |= SplineFlags1.MethodFitPoints;
			}

			//TODO: add KnotParametrization flag
		}
	}
}
