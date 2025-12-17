using ACadSharp.Objects;
using ACadSharp.Objects.Collections;
using Xunit;

namespace ACadSharp.Tests.Objects.Collections
{
	public class DictionaryVariableCollectionTests : ObjectDictionaryCollectionTests<DictionaryVariableCollection, DictionaryVariable>
	{
		[Fact]
		public void AddVariable()
		{
			CadDocument document = new CadDocument();
			var c = this.getDocumentCollection(document);

			c.AddVariable("hello", "value");

			Assert.NotEmpty(c);
			Assert.True(c.TryGet("hello", out DictionaryVariable variable));
			Assert.Equal("value", variable.Value);
			Assert.Equal("value", c.GetValue("hello"));

			Assert.Null(c.GetValue("goodbye"));

			c.AddVariable("hello", "new_value");
			Assert.Equal("new_value", c.GetValue("hello"));
		}

		protected override DictionaryVariableCollection getDocumentCollection(CadDocument doc)
		{
			return doc.DictionaryVariables;
		}
	}
}
