namespace ACadSharp.IO.Templates
{
	internal interface ICadDictionaryTemplate : ICadObjectTemplate
	{
		public CadObject CadObject { get; set; }
	}
}
