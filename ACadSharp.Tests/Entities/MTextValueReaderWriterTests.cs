using System;
using System.Collections.Generic;
using ACadSharp.Entities;
using Xunit;
using System.IO;
using Xunit.Abstractions;
using Newtonsoft.Json;

namespace ACadSharp.Tests.Entities
{
	public class MTextValueReaderWriterTests
	{
		private readonly ITestOutputHelper _output;
		private readonly Random _random;
		private readonly JsonSerializerSettings _jsonSettings;

		class MemoryConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				writer.WriteValue(value.ToString());
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
				JsonSerializer serializer)
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
			this._output = output;
			this._random = new Random();
			this._jsonSettings = new JsonSerializerSettings
			{
				Converters =
				{
					new MemoryConverter()
				},
			};

		}

		[Fact]
		public void SerializedTokensMatchDeserialized()
		{
			var writer = new MText.ValueWriter();
			var reader = new MText.ValueReader();

			for (int j = 0; j < 100; j++)
			{
				var source = this.randomTokens(1, 50);
				var serialized = writer.Serialize(source).ToString();
				var deserialized = reader.Deserialize(serialized);

				this.assertTokensEqual(source, deserialized);
			}
		}


		private MText.Token[] randomTokens(int min, int max)
		{
			var tokenCount = this._random.Next(min, max);
			var tokens = new List<MText.Token>(tokenCount);

			for (int i = 0; i < tokenCount; i++)
			{
				if (this._random.Next(0, 10) < 8)
				{
					tokens.Add(new MText.TokenValue(this.randomFormat(), randomString().AsMemory()));
				}
				else
				{
					tokens.Add(MTextValueTestData.CreateTokenFraction(
						this.randomFormat(), this._random.Next(0, 2) == 0 ? randomString() : null,
						this._random.Next(0, 2) == 0 ? randomString() : null,
						(MText.TokenFraction.Divider)this._random.Next(0, 3)));
				}
			}

			return tokens.ToArray();
		}

		private MText.Format randomFormat()
		{
			var format = new MText.Format()
			{
				//IsHeightRelative = Convert.ToBoolean(_random.Next(0, 2)),
				IsOverline = Convert.ToBoolean(this._random.Next(0, 2)),
				IsStrikeThrough = Convert.ToBoolean(this._random.Next(0, 2)),
				IsUnderline = Convert.ToBoolean(this._random.Next(0, 2)),
			};

			if (this._random.Next(0, 2) == 0)
				format.Align = (MText.Format.Alignment)this._random.Next(0, 3);

			if (this._random.Next(0, 2) == 0)
				format.Height = (float)Math.Round((this._random.NextDouble() * 60), 4);

			if (this._random.Next(0, 2) == 0)
				format.Obliquing = (float)(this._random.NextDouble() * 20 * (this._random.Next(0, 2) == 0 ? -1 : 1));

			if (this._random.Next(0, 2) == 0)
				format.Tracking = (float)(this._random.NextDouble() * 5);

			if (this._random.Next(0, 2) == 0)
				format.Width = (float)(this._random.NextDouble() * 4);

			if (this._random.Next(0, 2) == 0)
				format.Color = this._random.Next(0, 2) == 0
					? Color.FromTrueColor(this._random.Next(0, 1 << 24))
					: new Color((short)this._random.Next(0, 257));

			var paragraphCount = this._random.Next(0, 5);
			for (int i = 0; i < paragraphCount; i++)
			{
				format.Paragraph.Add(randomString().AsMemory());
			}

			format.Font.IsItalic = Convert.ToBoolean(this._random.Next(0, 2));
			format.Font.IsBold = Convert.ToBoolean(this._random.Next(0, 2));
			format.Font.CodePage = this._random.Next(0, 256);
			format.Font.FontFamily = randomString().AsMemory();
			format.Font.Pitch = this._random.Next(0, 10);

			return format;
		}

		private static string randomString()
		{
			return Path.GetRandomFileName();
		}


		private void assertTokensEqual(MText.Token[] expected, MText.Token[] actual)
		{
			Assert.Equal(expected.Length, actual.Length);

			for (int i = 0; i < expected.Length; i++)
			{
				// Format
				if (!Equals(expected[i].Format, actual[i].Format))
				{
					this.outputJson(expected, actual);
				}

				if (expected[i] is MText.TokenValue expectedValue
				    && actual[i] is MText.TokenValue actualValue
				    && expectedValue.Equals(actualValue))
				{
					return;
				}
				else if (expected[i] is MText.TokenFraction expectedFraction
				         && actual[i] is MText.TokenFraction actualFraction
				         && expectedFraction.Equals(actualFraction))
				{
					return;
				}
				else
				{
					this.outputJson(expected, actual);
				}
			}
		}

		private void outputJson(MText.Token[] expected, MText.Token[] actual)
		{
			var expectedJson = JsonConvert.SerializeObject(expected, this._jsonSettings);
			var otherJson = JsonConvert.SerializeObject(actual, this._jsonSettings);
			this._output.WriteLine("Expected:");
			this._output.WriteLine(expectedJson);
			this._output.WriteLine("Actual:");
			this._output.WriteLine(otherJson);
			Assert.Equal(expectedJson, otherJson);
		}
	}
}
