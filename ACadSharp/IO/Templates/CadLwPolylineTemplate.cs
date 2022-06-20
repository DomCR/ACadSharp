using ACadSharp.Entities;
using CSMath;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal class CadLwPolylineTemplate : CadEntityTemplate
	{
		public CadLwPolylineTemplate() : base(new LwPolyline()) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return found;

			LwPolyline lw = this.CadObject as LwPolyline;

			LwPolyline.Vertex last = lw.Vertices.LastOrDefault();

			switch (dxfcode)
			{
				case 10:
					lw.Vertices.Add(new LwPolyline.Vertex(new CSMath.XY((double)value, 0)));
					return true;
				case 20:
					if (last is not null)
					{
						last.Location = new CSMath.XY(last.Location.X, (double)value);
					}
					return true;
				case 40:
					if (last is not null)
					{
						last.StartWidth = (double)value;
					}
					return true;
				case 41:
					if (last is not null)
					{
						last.EndWidth = (double)value;
					}
					return true;
				case 42:
					if (last is not null)
					{
						last.Bulge = (double)value;
					}
					return true;
			}

			return found;
		}
	}
}
