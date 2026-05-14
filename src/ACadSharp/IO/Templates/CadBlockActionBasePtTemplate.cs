using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal abstract class CadBlockActionBasePtTemplate : CadBlockActionTemplate
	{
		public CadBlockActionBasePtTemplate(BlockActionBasePt blockAction)
			: base(blockAction)
		{
		}
	}
}