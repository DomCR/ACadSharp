using CSMath;
using Xunit;
using ACadSharp.Entities;
using ACadSharp.IO.Json;

#if NET
using System.Text.Json;
using System.Text.Json.Nodes;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace ACadSharp.Tests.IO.Json;

public class JsonConverterTests
{
	[Fact]
	public void EntityToJsonTest()
	{
		Line line = new Line(new XYZ(0, 0, 0), new XYZ(10, 10, 0));

#if NET
		string json = CadJsonConverter.Serialize(line, new JsonSerializerOptions
		{
			IgnoreReadOnlyProperties = true,
			IgnoreReadOnlyFields = true,
		});

		JsonObject obj = JsonNode.Parse(json).AsObject();
#else
		string json = CadJsonConverter.Serialize(line, new JsonSerializerSettings
		{
		});

		JObject obj = JObject.Parse(json);
#endif

		this.assertCadObjectJson(obj);

		Assert.True(obj.ContainsKey(nameof(Line.StartPoint)));
		Assert.True(obj.ContainsKey(nameof(Line.EndPoint)));
	}

#if NET
	private void assertCadObjectJson(JsonObject obj)
#else
	private void assertCadObjectJson(JObject obj)
#endif
	{
		Assert.True(obj.ContainsKey(nameof(CadObject.Handle)));
		Assert.True(obj.ContainsKey(nameof(CadObject.Owner)));
		Assert.True(obj.ContainsKey(nameof(CadObject.XDictionary)));
	}
}
