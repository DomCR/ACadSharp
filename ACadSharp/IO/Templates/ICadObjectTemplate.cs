using ACadSharp.IO.DWG;

namespace ACadSharp.IO.Templates
{
	internal interface ICadObjectTemplate
	{
		void Build(CadDocumentBuilder builder);
	}
}
