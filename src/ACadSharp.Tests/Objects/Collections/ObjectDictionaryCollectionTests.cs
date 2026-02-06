using ACadSharp.Objects;
using ACadSharp.Objects.Collections;
using Xunit;

namespace ACadSharp.Tests.Objects.Collections
{
	public abstract class ObjectDictionaryCollectionTests<T, R>
		where T : ObjectDictionaryCollection<R>
		where R : NonGraphicalObject
	{
		[Fact]
		public void InitCollection()
		{
			CadDocument doc = new CadDocument();
			Assert.NotNull(doc.Scales);
		}

		protected abstract T getDocumentCollection(CadDocument doc);
	}
}
