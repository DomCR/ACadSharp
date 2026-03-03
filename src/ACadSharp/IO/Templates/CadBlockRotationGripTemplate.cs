using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockRotationGripTemplate : CadBlockGripTemplate
	{
		public CadBlockRotationGripTemplate() : base(new BlockRotationGrip())
		{
		}

		public CadBlockRotationGripTemplate(BlockRotationGrip grip) : base(grip)
		{
		}
	}
}