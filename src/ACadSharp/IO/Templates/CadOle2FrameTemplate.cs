using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadOle2FrameTemplate : CadEntityTemplate<Ole2Frame>
	{
		public CadOle2FrameTemplate() : base(new Ole2Frame())
		{
		}
	}
}