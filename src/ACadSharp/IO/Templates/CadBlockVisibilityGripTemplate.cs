using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockVisibilityGripTemplate : CadBlockGripTemplate
	{
		public CadBlockVisibilityGripTemplate() : base(new BlockVisibilityGrip())
		{
		}

		public CadBlockVisibilityGripTemplate(BlockVisibilityGrip grip) : base(grip)
		{
		}
	}
}