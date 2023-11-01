using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadXRecordTemplate : CadTemplate<XRecord>
	{
		public CadXRecordTemplate() : base(new XRecord()) { }

		public CadXRecordTemplate(XRecord cadObject) : base(cadObject) { }
	}
}
