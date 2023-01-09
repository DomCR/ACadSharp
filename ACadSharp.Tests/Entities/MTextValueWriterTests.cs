using System;
using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.IO;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueWriterTests
    {

        [Theory, MemberData(nameof(MTextValueTestData.EscapesData), MemberType = typeof(MTextValueTestData))]
        public void Escapes(MTextValueTestData.TextData data)
        {
            var reader = new MText.ValueWriter();
            var parts = reader.Seralize(new[] { new MText.TokenValue(data.Decoded) });
            Assert.Equal(data.Encoded, string.Concat(parts));
        }
        
        [Theory, MemberData(nameof(MTextValueTestData.ReadsTextData), MemberType = typeof(MTextValueTestData))]
        public void ReadsText(MTextValueTestData.TextData data)
        {
            var reader = new MText.ValueWriter();
            var parts = reader.Seralize(new[] { new MText.TokenValue(data.Decoded) });
            Assert.Equal(data.Encoded, string.Concat(parts));
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


        private void TestFormatData(MTextValueTestData.FormatData data)
        {
            var reader = new MText.ValueWriter();
            var parts = reader.Seralize(data.Decoded);
            Assert.Equal(data.Encoded, string.Concat(parts));
        }
    }
}
