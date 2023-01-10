using System;
using System.Collections.Generic;
using ACadSharp.Entities;
using System.Linq;
using Xunit;
using System.IO;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueReaderWriterTests
    {
        private readonly Random _random;

        public MTextValueReaderWriterTests()
        {
            _random = new Random();
        }
        
        [Fact]
        public void Fractions(MTextValueTestData.FormatData data)
        {
            var writer = new MText.ValueWriter();
            var reader = new MText.ValueReader();


        }

        private MText.Token[] RandomTokens(int min, int max)
        {
            var tokenCount = _random.Next(min, max);
            var tokens = new List<MText.Token>(tokenCount);

            for (int i = 0; i < tokenCount; i++)
            {
                switch (_random.Next(0, 2))
                {
                    case 0:
                        tokens.Add(new MText.TokenValue(new MText.Format(), RandomString().AsMemory()));
                        break;
                        
                    case 1:
                        break;
                }
                tokens.Add();
            }
        }

        private MText.Format RandomFormat()
        {
            var format = new MText.Format()
            {
                IsHeightRelative = Convert.ToBoolean(_random.Next(0, 2)),
                Align = (MText.Format.Alignment)_random.Next(0, 3),
                Color = _random.Next(0, 2) == 0 
                    ? Color.FromTrueColor(_random.Next()) 
                    : new Color((short)_random.Next(0, 257)),
                Height = (float)(_random.NextDouble() * 60),
                IsOverline = Convert.ToBoolean(_random.Next(0, 2)),
                IsStrikeThrough = Convert.ToBoolean(_random.Next(0, 2)),
                IsUnderline = Convert.ToBoolean(_random.Next(0, 2)),
                Obliquing = (float)(_random.NextDouble() * 20),
                Tracking = (float)(_random.NextDouble() * 5),
                Width = (float)(_random.NextDouble() * 4)
            };
        }

        public static string RandomString()
        {
            return Path.GetRandomFileName();
        }
    }
}
