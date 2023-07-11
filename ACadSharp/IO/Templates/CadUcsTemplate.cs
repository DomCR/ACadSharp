using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadUcsTemplate : CadTableEntryTemplate<UCS>
	{
		public CadUcsTemplate() : base(new UCS()) { }

		public CadUcsTemplate(UCS entry) : base(entry) { }
	}
}
