using System;
using ACadSharp.Entities;
using ACadSharp.IO.DXF;

namespace ACadSharp.IO.Templates
{
	[Obsolete]
	internal class CadArcTemplate : CadEntityTemplate
	{

		public CadArcTemplate() : base(new Arc()) { }

		public CadArcTemplate(Entity entity) : base(entity) { }


        public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			return;

            if (builder is DxfDocumentBuilder && this.CadObject is Arc arc)
            {
                arc.StartAngle *= MathUtils.DegToRad;
                arc.EndAngle *= MathUtils.DegToRad;
            }
		}
	}
}
