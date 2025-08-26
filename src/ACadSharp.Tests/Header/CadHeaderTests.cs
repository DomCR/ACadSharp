using ACadSharp.Header;
using ACadSharp.Objects;
using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests.Header
{
	public class CadHeaderTests
	{
		[Fact]
		public void CadHeaderDefaultTest()
		{
			CadHeader header = new CadHeader();

			Assert.NotNull(header.CurrentLayer);
			Assert.True(header.CurrentLayer.Name == header.CurrentLayerName);
			Assert.True(header.CurrentLayer.Name == Layer.DefaultName);
			Assert.True(header.CurrentLayer.Handle == 0);
			Assert.Null(header.CurrentLayer.Owner);
		}

		[Fact]
		public void CadHeaderCurrentLayerTest()
		{
			CadDocument document = new CadDocument();
			CadHeader header = new CadHeader(document);

			Assert.NotNull(header.CurrentLayer);
			Assert.True(header.CurrentLayer.Name == header.CurrentLayerName, "Name does not match");
			Assert.True(header.CurrentLayer.Name == Layer.DefaultName, "Name does not match");
			Assert.True(header.CurrentLayer.Handle == document.Layers[header.CurrentLayerName].Handle, "Handle does not match");
			Assert.Equal(document.Layers, header.CurrentLayer.Owner);
		}

		[Fact]
		public void CadHeaderCurrentCurrentTextStyleTest()
		{
			CadDocument document = new CadDocument();
			CadHeader header = new CadHeader(document);

			Assert.NotNull(header.CurrentTextStyle);
			Assert.True(header.CurrentTextStyle.Name == header.TextStyleName, "Name does not match");
			Assert.True(header.CurrentTextStyle.Name == TextStyle.DefaultName, "Name does not match");
			Assert.True(header.CurrentTextStyle.Handle == document.TextStyles[header.TextStyleName].Handle, "Handle does not match");
			Assert.Equal(document.TextStyles, header.CurrentTextStyle.Owner);
		}

		[Fact]
		public void CadHeaderCurrentCurrentMultiLeaderStyleTest()
		{
			CadDocument document = new CadDocument();
			CadHeader header = new CadHeader(document);

			Assert.NotNull(header.CurrentMultiLeaderStyle);
			Assert.True(header.CurrentMultiLeaderStyle.Name == header.CurrentMultiLeaderStyleName, "Name does not match");
			Assert.True(header.CurrentMultiLeaderStyle.Name == MultiLeaderStyle.DefaultName, "Name does not match");
			Assert.True(header.CurrentMultiLeaderStyle.Handle == document.MLeaderStyles[header.CurrentMultiLeaderStyleName].Handle, "Handle does not match");
			Assert.Equal(document.MLeaderStyles, header.CurrentMultiLeaderStyle.Owner);

			Assert.True(((CadDictionary)document.RootDictionary[CadDictionary.VariableDictionary]).TryGetEntry(CadDictionary.CurrentMultiLeaderStyle, out DictionaryVariable currentMultiLeaderStyleNameVariable), "Dictionary variable for CMLEADERSTYLE does not exist.");
			Assert.True(currentMultiLeaderStyleNameVariable.Value == MultiLeaderStyle.DefaultName, "Dictionary variable CMLEADERSTYLE='standard' does not match.");

			string multiLeaderStyleName = "Test";
			document.MLeaderStyles.Add(new MultiLeaderStyle(multiLeaderStyleName));
			header.CurrentMultiLeaderStyleName = multiLeaderStyleName;
			Assert.True(document.MLeaderStyles.TryGetValue(multiLeaderStyleName, out MultiLeaderStyle multiLeaderStyle), "MultiLeaderStyle for new name 'Test' not created.");
			Assert.True(header.CurrentMultiLeaderStyle.Name == multiLeaderStyleName, "MultiLeaderStyle for new name 'Test' not set.");

			Assert.True(currentMultiLeaderStyleNameVariable.Value == multiLeaderStyleName, "Dictionary variable CMLEADERSTYLE='Test' does not match.");

			currentMultiLeaderStyleNameVariable.Value = MultiLeaderStyle.DefaultName;
			Assert.True(header.CurrentMultiLeaderStyleName == MultiLeaderStyle.DefaultName, "CurrentMultiLeaderStyleName for new name 'Standard' doe not match.");
			Assert.True(header.CurrentMultiLeaderStyle.Name == MultiLeaderStyle.DefaultName, "CurrentMultiLeaderStyle for new name 'Standard' not set.");
		}
	}
}
