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
            var parts = reader.Parse(data.Encoded);

            if (parts[0] is MText.TokenValue value1)
                Assert.Equal(data.Decoded, value1.CombinedValues);
        }

        [Theory, MemberData(nameof(MTextValueTestData.ReadsTextData), MemberType = typeof(MTextValueTestData))]
        public void ReadsText(MTextValueTestData.TextData data)
        {
            var reader = new MText.ValueReader();
            var parts = reader.Parse(data.Encoded);

            var combined = parts.OfType<MText.TokenValue>().Select(t => t.CombinedValues);
            Assert.Equal(data.Decoded, string.Concat(combined));
        }

        [Theory, MemberData(nameof(MTextValueTestData.FormatsData), MemberType = typeof(MTextValueTestData))]
        public void Formats(MTextValueTestData.FormatData data)
        {
            TestFormatData(data);
        }

        [Theory, MemberData(nameof(MTextValueTestData.FontsData), MemberType = typeof(MTextValueTestData))]
        public void Fonts(MTextValueTestData.FormatData data)
        {
            TestFormatData(data);
        }

        [Theory, MemberData(nameof(MTextValueTestData.ColorsData), MemberType = typeof(MTextValueTestData))]
        public void Colors(MTextValueTestData.FormatData data)
        {
            TestFormatData(data);
        }

        [Theory, MemberData(nameof(MTextValueTestData.FractionsData), MemberType = typeof(MTextValueTestData))]
        public void Fractions(MTextValueTestData.FormatData data)
        {
            TestFormatData(data);
        }


        [Theory, MemberData(nameof(MTextValueTestData.ParseData), MemberType = typeof(MTextValueTestData))]
        public void Parse(string text, int expectedParts)
        {
            var reader = new MText.ValueReader();
            var parts = reader.Parse(text);
        }

        private void TestFormatData(MTextValueTestData.FormatData data)
        {
            var reader = new MText.ValueReader();
            var parts = reader.Parse(data.Encoded, data.Format);

            if (data.Decoded == null)
            {
                Assert.Empty(parts);
                return;
            }

            Assert.Equal(data.Decoded!.Length, parts.Length);

            for (int i = 0; i < parts.Length; i++)
            {
                Assert.Equal(data.Decoded[i].Format, parts[i].Format);
                Assert.Equal(data.Decoded[i], parts[i]);
            }
        }
    }
}
