namespace ACadSharp.IO.Json;

internal class JsonDocumentBuilder : CadDocumentBuilder
{
	public override bool KeepUnknownEntities { get; }

	public override bool KeepUnknownNonGraphicalObjects { get; }

	public JsonDocumentBuilder(CadDocument document) : base(ACadVersion.Unknown, document)
	{
	}
}