using System;
using ACadSharp.Entities;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class MTextValueWriterTests
	{

		[Theory, MemberData(nameof(MTextValueTestData.EscapesData), MemberType = typeof(MTextValueTestData))]
		public void Escapes(MTextValueTestData.TextData data)
		{
			this.testTextData(data);
		}

		[Theory, MemberData(nameof(MTextValueTestData.ReadsTextData), MemberType = typeof(MTextValueTestData))]
		public void ReadsText(MTextValueTestData.TextData data)
		{
			this.testTextData(data);
		}

		[Theory, MemberData(nameof(MTextValueTestData.FormatsData), MemberType = typeof(MTextValueTestData))]
		public void Formats(MTextValueTestData.FormatData data)
		{
			this.testFormatData(data);
		}

		[Theory, MemberData(nameof(MTextValueTestData.FontsData), MemberType = typeof(MTextValueTestData))]
		public void Fonts(MTextValueTestData.FormatData data)
		{
			this.testFormatData(data);
		}

		[Theory, MemberData(nameof(MTextValueTestData.ColorsData), MemberType = typeof(MTextValueTestData))]
		public void Colors(MTextValueTestData.FormatData data)
		{
			this.testFormatData(data);
		}

		[Theory, MemberData(nameof(MTextValueTestData.FractionsData), MemberType = typeof(MTextValueTestData))]
		public void Fractions(MTextValueTestData.FormatData data)
		{
			// Skip the null decodes
			if (data.Decoded == null)
				return;
			this.testFormatData(data);
		}

		private void testTextData(MTextValueTestData.TextData data)
		{
			var writer = new MText.ValueWriter();
			var parts = writer.Serialize(new MText.Token[] { new MText.TokenValue(data.Decoded) });
			var concatParts = string.Concat(parts);
			for (int i = 0; i < data.Encoded.Length; i++)
			{
				if (concatParts == data.Encoded[i])
				{
					// Passes
					return;
				}
			}

			Assert.Equal(data.Encoded.FirstOrDefault(), concatParts);
		}

		private void testFormatData(MTextValueTestData.FormatData data)
		{
			if (data.Decoded == null)
				throw new InvalidOperationException();

			var reader = new MText.ValueWriter();
			var parts = reader.Serialize(data.Decoded);

			var concatParts = string.Concat(parts);
			for (int i = 0; i < data.Encoded.Length; i++)
			{
				if (concatParts == data.Encoded[i])
				{
					// Passes
					return;
				}
			}

			Assert.Equal(data.Encoded.FirstOrDefault(), concatParts);
		}
	}
}
