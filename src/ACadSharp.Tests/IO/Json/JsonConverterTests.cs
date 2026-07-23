using CSMath;
using Xunit;
using ACadSharp.Entities;
using ACadSharp.IO.Json;
using Xunit.Abstractions;
using System;
using ACadSharp.Tables;
using System.Linq;
using ACadSharp.Tests.Common;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ACadSharp.Tests.IO.Json;

public class JsonConverterTests
{
	public static readonly TheoryData<Type> EntityTypes = new TheoryData<Type>();

	private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
	};

	private readonly ITestOutputHelper _output;

	static JsonConverterTests()
	{
		var d = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "ACadSharp.dll");
		foreach (var item in d.GetTypes().Where(i => !i.IsAbstract && i.IsPublic))
		{
			if (item.IsSubclassOf(typeof(Entity)) && item.GetConstructor(Array.Empty<Type>()) != null)
			{
				EntityTypes.Add(item);
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

		string json = CadJsonConverter.Serialize(blk, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();

		this._output.WriteLine(json);
		this.assertCadObjectJson(obj);
	}

	[Fact]
	public void CadDocumentToJsonTest()
	{
		CadDocument doc = new();
		doc.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(10, 10, 0)));

		string json = CadJsonConverter.Serialize(doc, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();

		this._output.WriteLine(json);
	}

	[Theory]
	[MemberData(nameof(EntityTypes))]
	public void EntityToJsonTest(Type type)
	{
		Entity entity = (Entity)Factory.CreateObject(type);

		string json = CadJsonConverter.Serialize(entity, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();

		this._output.WriteLine(json);
		this.assertCadObjectJson(obj);
		this.assertEntityJson(obj);
	}

	[Fact]
	public void JsonToCadDocumentTest()
	{
		CadDocument doc = new();
		doc.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(10, 10, 0)));

		string json = CadJsonConverter.Serialize(doc, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();

		this._output.WriteLine(json);

		var result = CadJsonConverter.DeserializeDocument(json);
	}

	[Theory]
	[MemberData(nameof(EntityTypes))]
	public void JsonToEntityTest(Type type)
	{
		Entity entity = (Entity)Factory.CreateObject(type);

		string json = CadJsonConverter.Serialize(entity, _jsonOptions);

		JsonObject obj = JsonNode.Parse(json).AsObject();

		this._output.WriteLine(json);

		var cadobj = CadJsonConverter.Deserialize(json, type);

		Assert.Equal(type, cadobj.GetType());
		EntityComparator.Equals(entity, (Entity)cadobj);
	}

	private void assertCadObjectJson(JsonObject obj)
	{
		Assert.True(obj.ContainsKey(nameof(CadObject.ObjectName)));
		Assert.True(obj.ContainsKey(nameof(CadObject.SubclassMarker)));
		Assert.True(obj.ContainsKey(nameof(CadObject.Handle)));
		Assert.True(obj.ContainsKey(nameof(CadObject.Owner)));
		Assert.True(obj.ContainsKey(nameof(CadObject.XDictionary)));
	}

	private void assertEntityJson(JsonObject obj)
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