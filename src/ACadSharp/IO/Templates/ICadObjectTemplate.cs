namespace ACadSharp.IO.Templates
{
	internal interface ICadObjectTemplate : ICadTemplate
	{
		CadObject CadObject { get; }
	}
}
