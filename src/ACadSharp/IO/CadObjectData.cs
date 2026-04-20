namespace ACadSharp.IO;

public readonly struct CadObjectData
{
	public ulong Handle { get; }

	public string Type { get; }

	public CadObjectData(CadObject cadObject) : this()
	{
		this.Handle = cadObject.Handle;
		this.Type = cadObject.ObjectName;
	}
}