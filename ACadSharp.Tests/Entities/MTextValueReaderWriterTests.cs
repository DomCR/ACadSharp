using System;
using System.Collections.Generic;
using ACadSharp.Entities;
using System.Linq;
using Xunit;
using System.IO;
using Xunit.Abstractions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ACadSharp.Tests.Entities
{
    public class MTextValueReaderWriterTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Random _random;
        private readonly JsonSerializerSettings _jsonSettings;

        class RoundedDecimalJsonConverter : JsonConverter
        {
            public override bool CanRead => false;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(float?) || objectType == typeof(double?));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if(value is float valueFloat)
                    writer.WriteValue(Math.Truncate(valueFloat * 10000) / 10000);
                else if (value is double valueDouble)
                    writer.WriteValue(Math.Truncate(valueDouble * 10000) / 10000);
            }
        }

        class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

                if (type == typeof(Color))
                {
                    var p = props.Where(p =>
                    {
                        return p.PropertyName == "TrueColor"
                               || p.PropertyName == "Index";
                    }).ToList();

                    return p;
                }

                return props.Where(p =>
                {
                    return !p.PropertyName.EndsWith("Combined")
                           && p.PropertyName != "CombinedValues";
                }).ToList();
            }
        }

        class MemoryConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (existingValue is string valueString)
                    return valueString.AsMemory();

                return null;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ReadOnlyMemory<char>);
            }
        }

        public MTextValueReaderWriterTests(ITestOutputHelper output)
        {
            _output = output;
            _random = new Random();
            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new WritablePropertiesOnlyResolver(),
                Converters =
                {
                    new MemoryConverter(),
                    new RoundedDecimalJsonConverter()
                },
                
                //Formatting = Formatting.Indented
            };

        }

        [Fact]
        public void RandomTokenTests()
        {
            var writer = new MText.ValueWriter();
            var reader = new MText.ValueReader();
            
            for (int j = 0; j < 2000; j++)
            {
                var source = RandomTokens(1, 2);
                var sourceJson = JsonConvert.SerializeObject(source, _jsonSettings);
                var serialized = writer.Serialize(source).ToString();

                var deserialized = reader.Deserialize(serialized);
                var deserializedJson = JsonConvert.SerializeObject(deserialized, _jsonSettings);

                try
                {
                    Assert.Equal(sourceJson, deserializedJson);
                }
                catch (Exception e)
                {
                    _output.WriteLine("Source JSON:");
                    _output.WriteLine(sourceJson);

                    _output.WriteLine("Serialized:");
                    _output.WriteLine(serialized);

                    _output.WriteLine("Deserialized JSON:");
                    _output.WriteLine(deserializedJson);
                }
                
                Assert.Equal(source.Length, deserialized.Length);
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
                //IsHeightRelative = Convert.ToBoolean(_random.Next(0, 2)),
                IsOverline = Convert.ToBoolean(_random.Next(0, 2)),
                IsStrikeThrough = Convert.ToBoolean(_random.Next(0, 2)),
                IsUnderline = Convert.ToBoolean(_random.Next(0, 2)),
            };

            if (_random.Next(0, 2) == 0)
                format.Align = (MText.Format.Alignment)_random.Next(0, 3);

            //if (_random.Next(0, 2) == 0)
            //    format.Height = (float)Math.Round((_random.NextDouble() * 60), 4);

            if (_random.Next(0, 2) == 0)
                format.Obliquing = (float)(_random.NextDouble() * 20 * (_random.Next(0, 2) == 0 ? -1 : 1));

            if (_random.Next(0, 2) == 0)
                format.Tracking = (float)(_random.NextDouble() * 5);

            if (_random.Next(0, 1) == 0)
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
