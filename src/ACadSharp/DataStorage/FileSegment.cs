namespace ACadSharp.DataStorage;

internal class FileSegment
{
	public FileSegmentHeader Header { get; set; }

	public FileSegment(string name)
	{
		this.Header = new FileSegmentHeader(name);
	}

	public override string ToString()
	{
		return $"{this.Header.Name}";
	}
}
