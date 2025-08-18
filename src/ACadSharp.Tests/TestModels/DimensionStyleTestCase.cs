using ACadSharp.Entities;
using ACadSharp.Tables;
using Xunit.Abstractions;

namespace ACadSharp.Tests.TestModels
{
	//Json serialization needs to be fixed for the XUnit serialization to work
	public class DimensionStyleTestCase : IXunitSerializable
	{
		public Dimension Dimension { get; private set; }

		public string Expected { get; private set; }

		public DimensionStyle Style { get; private set; }

		public DimensionStyleTestCase()
		{ }

		public DimensionStyleTestCase(DimensionStyle style, Dimension dimension, string expected)
		{
			this.Style = style;
			this.Dimension = dimension;
			this.Expected = expected;
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
#if !NETFRAMEWORK
			this.Dimension = System.Text.Json.JsonSerializer.Deserialize<Dimension>(
				info.GetValue<string>(nameof(this.Dimension)));
			this.Style = System.Text.Json.JsonSerializer.Deserialize<DimensionStyle>(
				info.GetValue<string>(nameof(this.Style)));
			this.Expected = info.GetValue<string>(nameof(this.Expected));
#endif
		}

		public void Serialize(IXunitSerializationInfo info)
		{
#if !NETFRAMEWORK
			info.AddValue(nameof(Dimension), System.Text.Json.JsonSerializer.Serialize(Dimension));
			info.AddValue(nameof(Style), System.Text.Json.JsonSerializer.Serialize(Style));
			info.AddValue(nameof(Expected), Expected);
#endif
		}

		public override string ToString()
		{
			return $"{this.Dimension}|{this.Expected}";
		}
	}
}