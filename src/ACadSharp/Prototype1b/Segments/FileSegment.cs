namespace ACadSharp.Prototype1b.Segments;

public class FileSegment : IPrototype1bSegment
{
	public FileSegmentHeader Header { get; set; }

	public const string Blob01Name = "blob01";

	public const string DataIndexName = "datidx";

	public const string DataName = "_data_";

	public const string FreeSpaceName = "freesp";

	public const string PreviousSaveName = "prvsav";

	public const string SchemaDataName = "schdat";

	public const string SchemaIndexName = "schidx";

	public const string SchemaSearchName = "search";

	public const string SegmentIndexName = "segidx";
}