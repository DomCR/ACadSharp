using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadLwPolylineTemplate : CadEntityTemplate
	{
		public CadLwPolylineTemplate() : base(new LwPolyline()) { }
	}
}
