namespace ACadSharp.DataStorage;

internal class FileSegment
{
	public FileSegmentHeader Header { get; set; }

	public override string ToString()
	{
		return $"{this.Header.Name}";
	}
}
