namespace ACadSharp.IO.Templates
{
	internal interface ICadTemplate
	{
		void Build(CadDocumentBuilder builder);
	}

	internal interface ICadObjectTemplate : ICadTemplate
	{
		CadObject CadObject { get; }
	}
}
