using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadUcsTemplate : CadTableEntryTemplate<UCS>
	{
		public CadUcsTemplate(UCS entry) : base(entry) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return true;

			switch (dxfcode)
			{
				//NOTE: Undocumented code
				case 79:
					found = true;
					break;
				default:
					break;
			}

			return found;
		}
	}
}
