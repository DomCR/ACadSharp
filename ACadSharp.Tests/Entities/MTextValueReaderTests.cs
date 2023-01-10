using ACadSharp.Entities;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueReaderTests
    {
        [Theory, MemberData(nameof(MTextValueTestData.EscapesData), MemberType = typeof(MTextValueTestData))]
        public void Escapes(MTextValueTestData.TextData data)
        {
            var reader = new MText.ValueReader();

            for (int i = 0; i < data.Encoded.Length; i++)
            {
                var parts = reader.Deserialize(data.Encoded[i]);

                if (parts[0] is MText.TokenValue value1)
                    Assert.Equal(data.Decoded, value1.CombinedValues);
            }

        }

        [Theory, MemberData(nameof(MTextValueTestData.ReadsTextData), MemberType = typeof(MTextValueTestData))]
        public void ReadsText(MTextValueTestData.TextData data)
        {
            var reader = new MText.ValueReader();
            for (int i = 0; i < data.Encoded.Length; i++)
            {
                var parts = reader.Deserialize(data.Encoded[i]);

                var combined = parts.OfType<MText.TokenValue>().Select(t => t.CombinedValues);
                Assert.Equal(data.Decoded, string.Concat(combined));
            }
        }

        [Theory, MemberData(nameof(MTextValueTestData.FormatsData), MemberType = typeof(MTextValueTestData))]
        public void Formats(MTextValueTestData.FormatData data)
        {
            testFormatData(data);
        }

        [Theory, MemberData(nameof(MTextValueTestData.FontsData), MemberType = typeof(MTextValueTestData))]
        public void Fonts(MTextValueTestData.FormatData data)
        {
            testFormatData(data);
        }

        [Theory, MemberData(nameof(MTextValueTestData.ColorsData), MemberType = typeof(MTextValueTestData))]
        public void Colors(MTextValueTestData.FormatData data)
        {
            testFormatData(data);
        }

        [Theory, MemberData(nameof(MTextValueTestData.FractionsData), MemberType = typeof(MTextValueTestData))]
        public void Fractions(MTextValueTestData.FormatData data)
        {
            testFormatData(data);
        }


        [Theory, MemberData(nameof(MTextValueTestData.ParseData), MemberType = typeof(MTextValueTestData))]
        public void Deserializes(string text, int expectedParts)
        {
            var reader = new MText.ValueReader();
            reader.Deserialize(text);
        }

        private static void testFormatData(MTextValueTestData.FormatData data)
        {
            var reader = new MText.ValueReader();

            for (int i = 0; i < data.Encoded.Length; i++)
            {
                var parts = reader.Deserialize(data.Encoded[i], data.Format);

                if (data.Decoded == null)
                {
                    Assert.Empty(parts);
                    return;
                }

                Assert.Equal(data.Decoded!.Length, parts.Length);

                for (int j = 0; j < parts.Length; j++)
                {
                    Assert.Equal(data.Decoded[j].Format, parts[j].Format);
                    Assert.Equal(data.Decoded[j], parts[j]);
                }
            }

        }
    }
}
