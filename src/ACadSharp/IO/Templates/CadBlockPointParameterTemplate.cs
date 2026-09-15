using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal class CadBlockPointParameterTemplate : CadBlock1PtParameterTemplate
{
	public BlockPointParameter BlockPointParameter { get { return this.CadObject as BlockPointParameter; } }

	public CadBlockPointParameterTemplate() : base(new BlockPointParameter())
	{ }

	public CadBlockPointParameterTemplate(BlockPointParameter cadObject)
		: base(cadObject)
	{
	}
}