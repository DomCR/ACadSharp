using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadXRecordTemplate : CadTemplate<XRecrod>
	{
		public CadXRecordTemplate() : base(new XRecrod()) { }

		public CadXRecordTemplate(XRecrod cadObject) : base(cadObject) { }
	}
}
