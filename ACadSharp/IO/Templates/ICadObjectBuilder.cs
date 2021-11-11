using ACadSharp.IO.DWG;

namespace ACadSharp.IO.Templates
{
	internal interface ICadObjectBuilder
	{
		void Build(DwgDocumentBuilder builder);
	}
}
