using ACadSharp.Entities;
using ACadSharp.IO;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfEncodingTests
{
	[Theory]
	[InlineData("en-US")]
	[InlineData("tr-TR")]
	[InlineData("az-Latn-AZ")]
	public void ReadWindows1254TextIgnoresCurrentCulture(string cultureName)
	{
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

#if !NET48
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			string text = "Şişe";
			string dxf = string.Join("\n",
				"0", "SECTION",
				"2", "HEADER",
				"9", "$ACADVER",
				"1", "AC1018",
				"9", "$DWGCODEPAGE",
				"3", "ANSI_1254",
				"0", "ENDSEC",
				"0", "SECTION",
				"2", "ENTITIES",
				"0", "TEXT",
				"5", "A1",
				"100", "AcDbEntity",
				"8", "0",
				"100", "AcDbText",
				"10", "0",
				"20", "0",
				"30", "0",
				"40", "1",
				"1", text,
				"0", "ENDSEC",
				"0", "EOF");

			using (MemoryStream stream = new MemoryStream(Encoding.GetEncoding(1254).GetBytes(dxf)))
			using (DxfReader reader = new DxfReader(stream))
			{
				CadDocument document = reader.Read();
				TextEntity entity = Assert.Single(document.Entities.OfType<TextEntity>());
				Assert.Equal(text, entity.Value);
			}
		}
		finally
		{
			CultureInfo.CurrentCulture = originalCulture;
		}
	}
}
