using ACadSharp.Entities;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadSplineTemplate : CadEntityTemplate
	{
        private CSMath.XYZ _currentControlPoint = new CSMath.XYZ();
        private CSMath.XYZ _currentFitPoint = new CSMath.XYZ();

        public CadSplineTemplate(Spline entity) : base(entity) { }

        public override bool CheckDxfCode(int dxfcode, object value)
        {
            bool found = base.CheckDxfCode(dxfcode, value);
            if (found)
                return found;

            var spline = this.CadObject as Spline;

            switch(dxfcode)
            {
                //--- Knot
                case 40:
                    spline.Knots.Add((double)value);
                    return true;

                //--- Weight
                case 41:
                    spline.Weights.Add((double)value);
                    return true;

                //--- ControlPoint
                case 10:
                    this._currentControlPoint = new CSMath.XYZ(
                        (double)value,
                        _currentControlPoint.Y,
                        _currentControlPoint.Z
                        );
                    return true;
                case 20:
                    this._currentControlPoint = new CSMath.XYZ(                        
                        _currentControlPoint.X,
                        (double)value,
                        _currentControlPoint.Z
                        );
                    return true;
                case 30:
                    this._currentControlPoint = new CSMath.XYZ(
                        _currentControlPoint.X,
                        _currentControlPoint.Y,
                        (double)value
                        );
                    spline.ControlPoints.Add(this._currentControlPoint);
                    return true;

                //--- FitPoint
                case 11:
                    this._currentFitPoint = new CSMath.XYZ(
                        (double)value,
                        _currentFitPoint.Y,
                        _currentFitPoint.Z
                        );
                    return true;
                case 21:
                    this._currentFitPoint = new CSMath.XYZ(
                        _currentFitPoint.X,
                        (double)value,
                        _currentFitPoint.Z
                        );
                    return true;
                case 31:
                    this._currentFitPoint = new CSMath.XYZ(
                        _currentFitPoint.X,
                        _currentFitPoint.Y,
                        (double)value
                        );
                    spline.FitPoints.Add(this._currentFitPoint);
                    return true;
            }

            return found;
        }
    }
}
