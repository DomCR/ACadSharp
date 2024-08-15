namespace ACadSharp.IO.Templates
{
	internal interface ICadObjectTemplate
	{
		CadObject CadObject { get; }

		void Build(CadDocumentBuilder builder);
	}
}
