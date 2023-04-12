using System;
using ACadSharp.Entities;
using ACadSharp.IO.DXF;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadArcTemplate : CadEntityTemplate
	{

		public CadArcTemplate(Entity entity) : base(entity) { }


        public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

            if (builder is DxfDocumentBuilder && this.CadObject is Arc arc)
            {
                arc.StartAngle *= MathUtils.DegToRad;
                arc.EndAngle *= MathUtils.DegToRad;
            }
		}
	}
}
