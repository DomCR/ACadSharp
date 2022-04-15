using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadXRecordTemplate : CadTemplate<XRecrod>
	{
		public CadXRecordTemplate(XRecrod cadObject) : base(cadObject) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			//1-369 (except 5 and 105)
			//These values can be used by an application in any way

			//TODO: Finsih cadXrecordtemplate

			if (dxfcode == 5 || dxfcode == 105 || dxfcode > 369)
				return false;
			else
				return true;
		}
	}
}
