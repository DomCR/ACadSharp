using CSMath;
using Xunit;
using ACadSharp.Entities;
using ACadSharp.IO.Json;
using Xunit.Abstractions;
using System.Collections.Generic;
using System;
using ACadSharp.Tables;
using System.Linq;
using ACadSharp.Tests.Common;
using static ACadSharp.Objects.XRecord;

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
	public static readonly TheoryData<Type> Entities = new TheoryData<Type>();

#if NET
	private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
	};
#else
private readonly JsonSerializerSettings _jsonOptions = new JsonSerializerSettings
{
	Formatting = Formatting.Indented,
};
#endif

	private readonly ITestOutputHelper _output;

	static JsonConverterTests()
	{
		var d = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "ACadSharp.dll");
		foreach (var item in d.GetTypes().Where(i => !i.IsAbstract && i.IsPublic))
		{
			if (item.IsSubclassOf(typeof(Entity)) && item.GetConstructor(Array.Empty<Type>()) != null)
			{
				Entities.Add(item);
			}
		}
	}

	public JsonConverterTests(ITestOutputHelper output)
	{
		this._output = output;
	}

	[Fact]
	public void BlockRecordToJsonTest()
	{
		BlockRecord blk = new BlockRecord("my_block");
		blk.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(10, 10, 0)));

#if NET
		string json = CadJsonConverter.Serialize(blk, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();

#else
		string json = CadJsonConverter.Serialize(blk, _jsonOptions);

		JObject obj = JObject.Parse(json);
#endif

		this._output.WriteLine(json);
		this.assertCadObjectJson(obj);
	}

	[Fact]
	public void CadDocumentToJsonTest()
	{
		CadDocument doc = new();
		doc.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(10, 10, 0)));

#if NET
		string json = CadJsonConverter.Serialize(doc, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();
#else
		string json = CadJsonConverter.Serialize(doc, _jsonOptions);

		JObject obj = JObject.Parse(json);
#endif

		this._output.WriteLine(json);
	}

	[Theory]
	[MemberData(nameof(Entities))]
	public void EntityToJsonTest(Type type)
	{
		Entity entity = (Entity)Factory.CreateObject(type);

#if NET
		string json = CadJsonConverter.Serialize(entity, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();

#else
		string json = CadJsonConverter.Serialize(entity, _jsonOptions);

		JObject obj = JObject.Parse(json);
#endif

		this._output.WriteLine(json);
		this.assertCadObjectJson(obj);
		this.assertEntityJson(obj);
	}

#if NET

	private void assertCadObjectJson(JsonObject obj)
#else
	private void assertCadObjectJson(JObject obj)
#endif
	{
		Assert.True(obj.ContainsKey(nameof(CadObject.ObjectName)));
		Assert.True(obj.ContainsKey(nameof(CadObject.SubclassMarker)));
		Assert.True(obj.ContainsKey(nameof(CadObject.Handle)));
		Assert.True(obj.ContainsKey(nameof(CadObject.Owner)));
		Assert.True(obj.ContainsKey(nameof(CadObject.XDictionary)));
	}

#if NET

	private void assertEntityJson(JsonObject obj)
#else
	private void assertEntityJson(JObject obj)
#endif
	{
		Assert.True(obj.ContainsKey(nameof(Entity.Layer)));
		Assert.True(obj.ContainsKey(nameof(Entity.Color)));
		Assert.True(obj.ContainsKey(nameof(Entity.IsInvisible)));
		Assert.True(obj.ContainsKey(nameof(Entity.LineType)));
		Assert.True(obj.ContainsKey(nameof(Entity.LineTypeScale)));
		Assert.True(obj.ContainsKey(nameof(Entity.LineWeight)));
		Assert.True(obj.ContainsKey(nameof(Entity.Material)));
		Assert.True(obj.ContainsKey(nameof(Entity.Transparency)));
	}
}