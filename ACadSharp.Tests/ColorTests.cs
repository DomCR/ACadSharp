using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ACadSharp.Tests
{
	public class ColorTests
	{
		[Fact]
		public void IndexedColorProperties()
		{
            Assert.True(new Color(256).IsByLayer);
            Assert.True(new Color(0).IsByBlock);
        }

        private int Int32FromInt24(byte[] array)
        {
            if (BitConverter.IsLittleEndian)
                return array[0] | array[1] << 8 | array[2] << 16;
            else
                return array[0] << 16 | array[1] << 8 | array[2];
        }
        [Fact]
        public void ParsesTrueColors()
        {
            var random = new Random();
            byte[] colors = new byte[3];
            for (int i = 0; i < 1000; i++)
            {
                random.NextBytes(colors);

                var intColor = Int32FromInt24(colors);
                var color = new Color(colors);
                Assert.True(color.IsTrueColor, color.TrueColor.ToString() + $" Bytes:{colors[0]},{colors[1]},{colors[2]},");
                Assert.Equal(intColor, color.TrueColor);
                Assert.Equal(-1, color.Index);

                var rgb = color.GetTrueColorRgb();
                Assert.Equal(colors[0], rgb[0]);
                Assert.Equal(colors[1], rgb[1]);
                Assert.Equal(colors[2], rgb[2]);
            }
        }

        [Fact]
        public void Handles000TrueColor()
        {
            var color = new Color(new byte[] { 0, 0, 0 });

            Assert.True(color.IsTrueColor);
            Assert.Equal(0, color.TrueColor);
        }

        [Fact]
        public void HandlesIndexedColors()
        {
            for (short i = 0; i <= 256; i++)
            {
                var color = new Color(i);
                Assert.False(color.IsTrueColor);
                Assert.Equal(i, color.Index);
                Assert.Equal(-1, color.TrueColor);
                Assert.True(ReadOnlySpan<byte>.Empty.SequenceEqual(color.GetTrueColorRgb()));
            }
        }
    }
}
