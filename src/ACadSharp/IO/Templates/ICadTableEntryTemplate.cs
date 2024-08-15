namespace ACadSharp.IO.Templates
{
	internal interface ICadTableEntryTemplate : ICadObjectTemplate
	{
		string Type { get; }
		
		string Name { get; }
	}
}
