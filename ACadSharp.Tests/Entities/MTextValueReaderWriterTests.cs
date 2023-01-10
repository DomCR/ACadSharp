using System;
using System.Collections.Generic;
using ACadSharp.Entities;
using System.Linq;
using Xunit;
using System.IO;
using Xunit.Abstractions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueReaderWriterTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Random _random;

        public MTextValueReaderWriterTests(ITestOutputHelper output)
        {
            _output = output;
            _random = new Random();
        }

        [Fact]
        public void RandomTokenTests()
        {
            var writer = new MText.ValueWriter();
            var reader = new MText.ValueReader();

            for (int j = 0; j < 20; j++)
            {
                var tokens = RandomTokens(1, 2);
                var tokensJson = JsonConvert.SerializeObject(tokens);

                var serialized = writer.Serialize(tokens).ToString();

                var deserialized = reader.Deserialize(serialized);
                var deserializedJson = JsonConvert.SerializeObject(deserialized);

                Assert.Equal(tokensJson, deserializedJson);


                Assert.Equal(tokens.Length, deserialized.Length);

                for (int i = 0; i < deserialized.Length; i++)
                {
                    Assert.Equal(tokens[i], deserialized[i]);
                }

                var seralizedAgain = writer.Serialize(deserialized).ToString();
                _output.WriteLine(serialized);
                _output.WriteLine(seralizedAgain);
                Assert.Equal(serialized, seralizedAgain);
            }
            
        }


        private MText.Token[] RandomTokens(int min, int max)
        {
            var tokenCount = _random.Next(min, max);
            var tokens = new List<MText.Token>(tokenCount);

            for (int i = 0; i < tokenCount; i++)
            {
                if (_random.Next(0, 10) < 8)
                {
                    tokens.Add(new MText.TokenValue(RandomFormat(), RandomString().AsMemory()));
                }
                else
                {
                    tokens.Add(new MText.TokenFraction(
                        RandomFormat(),
                        _random.Next(0, 2) == 0 ? RandomString() : null,
                        _random.Next(0, 2) == 0 ? RandomString() : null,
                        (MText.TokenFraction.Divider)_random.Next(0, 3)));
                }
            }

            return tokens.ToArray();
        }

        private MText.Format RandomFormat()
        {
            var format = new MText.Format()
            {
                IsHeightRelative = Convert.ToBoolean(_random.Next(0, 2)),
                IsOverline = Convert.ToBoolean(_random.Next(0, 2)),
                IsStrikeThrough = Convert.ToBoolean(_random.Next(0, 2)),
                IsUnderline = Convert.ToBoolean(_random.Next(0, 2)),
            };

            if (_random.Next(0, 2) == 0)
                format.Align = (MText.Format.Alignment)_random.Next(0, 3);

            if (_random.Next(0, 2) == 0)
                format.Height = (float)(_random.NextDouble() * 60);

            if (_random.Next(0, 2) == 0)
                format.Obliquing = (float)(_random.NextDouble() * 20 * (_random.Next(0, 2) == 0 ? -1 : 1));

            if (_random.Next(0, 2) == 0)
                format.Tracking = (float)(_random.NextDouble() * 5);

            if (_random.Next(0, 2) == 0)
                format.Width = (float)(_random.NextDouble() * 4);

            if (_random.Next(0, 2) == 0)
                format.Color = _random.Next(0, 2) == 0
                    ? Color.FromTrueColor(_random.Next(0, 1 << 24))
                    : new Color((short)_random.Next(0, 257));

            var paragraphCount = _random.Next(0, 5);
            for (int i = 0; i < paragraphCount; i++)
            {
                format.Paragraph.Add(RandomString().AsMemory());
            }

            format.Font.IsItalic = Convert.ToBoolean(_random.Next(0, 2));
            format.Font.IsBold = Convert.ToBoolean(_random.Next(0, 2));
            format.Font.CodePage = _random.Next(0, 256);
            format.Font.FontFamily = RandomString().AsMemory();
            format.Font.Pitch = _random.Next(0, 10);

            return format;
        }

        public static string RandomString()
        {
            return Path.GetRandomFileName();
        }
    }
}
