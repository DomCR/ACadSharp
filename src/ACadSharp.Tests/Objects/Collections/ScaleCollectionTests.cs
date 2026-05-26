using ACadSharp.Objects;
using ACadSharp.Objects.Collections;

namespace ACadSharp.Tests.Objects.Collections
{
	public class ScaleCollectionTests : ObjectDictionaryCollectionTests<ScaleCollection, Scale>
	{
		protected override ScaleCollection getDocumentCollection(CadDocument doc)
		{
			return doc.Scales;
		}
	}
}
