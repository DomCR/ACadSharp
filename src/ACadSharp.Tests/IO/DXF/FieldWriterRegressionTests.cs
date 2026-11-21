using ACadSharp.IO;
using ACadSharp.Objects;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class FieldWriterRegressionTests
{
	private static (Dictionary<ulong, List<(int Code, string Value)>> Records, ulong Seed) write(CadDocument document)
	{
		using var stream = new MemoryStream();
		DxfWriter.Write(stream, document, false, new DxfWriterConfiguration { CloseStream = false });
		string[] lines = Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n").Split('\n');
		var pairs = new List<(int Code, string Value)>();
		for (int i = 0; i + 1 < lines.Length; i += 2)
			pairs.Add((int.Parse(lines[i], CultureInfo.InvariantCulture), lines[i + 1]));
		var records = new Dictionary<ulong, List<(int Code, string Value)>>();
		for (int i = 0; i < pairs.Count; i++)
		{
			if (pairs[i] != (0, "FIELD")) continue;
			int end = i + 1;
			while (end < pairs.Count && pairs[end].Code != 0) end++;
			var record = pairs.Skip(i + 1).Take(end - i - 1).ToList();
			// Add fails if a referenced FIELD was serialized more than once.
			records.Add(ulong.Parse(record.First(p => p.Code == 5).Value, NumberStyles.HexNumber), record);
		}
		int seed = pairs.FindIndex(p => p == (9, "$HANDSEED"));
		return (records, ulong.Parse(pairs[seed + 1].Value, NumberStyles.HexNumber));
	}

	private static void verify(CadDocument doc, Field[] fields, (Dictionary<ulong, List<(int Code, string Value)>> Records, ulong Seed) output)
	{
		Assert.Equal(fields.Length, output.Records.Count);
		foreach (var field in fields)
		{
			Assert.True(field.Handle > 0 && field.Handle < output.Seed);
			Assert.Same(field, doc.GetCadObject(field.Handle));
			Assert.Equal(field.Owner.Handle.ToString("X"), output.Records[field.Handle].First(p => p.Code == 330).Value);
			foreach (var child in field.Children)
				Assert.Contains((360, child.Handle.ToString("X")), output.Records[field.Handle]);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void AllocatesNestedFieldsBeforeHeaderAndWritesEachOnce(bool dictionaryRoot)
	{
		var doc = new CadDocument();
		var parent = new Field { FormatString = string.Empty, Name = "PARENT" };
		var child = new Field { FormatString = string.Empty }; var grandchild = new Field { FormatString = string.Empty };
		parent.Children.Add(child); child.Children.Add(grandchild);
		if (dictionaryRoot) doc.RootDictionary.Add(parent);
		else
		{
			var list = new FieldList { Name = "PORT_FIELDS" }; doc.RootDictionary.Add(list);
			list.Fields.Add(parent);
		}
		var fields = new[] { parent, child, grandchild };
		var first = write(doc); verify(doc, fields, first);
		var second = write(doc); verify(doc, fields, second);
		Assert.Equal(first.Seed, second.Seed);
		Assert.Same(parent, child.Owner); Assert.Same(child, grandchild.Owner);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ChildOwnersTakePrecedenceOverFieldListReferences(bool childFirst)
	{
		var doc = new CadDocument();
		var list = new FieldList { Name = "PORT_FIELDS" }; doc.RootDictionary.Add(list);
		var parent = new Field { FormatString = string.Empty, Handle = 10000 }; var child = new Field { FormatString = string.Empty, Handle = 10001 };
		parent.Children.Add(child);
		list.Fields.AddRange(childFirst ? new[] { child, parent } : new[] { parent, child });
		verify(doc, new[] { parent, child }, write(doc));
		Assert.Same(list, parent.Owner); Assert.Same(parent, child.Owner);
	}

	[Fact]
	public void PreservesDictionaryOwnerWhenReferencedFromMultipleLists()
	{
		var doc = new CadDocument(); var field = new Field { FormatString = string.Empty, Name = "FIELD" }; doc.RootDictionary.Add(field);
		foreach (string name in new[] { "FIRST", "SECOND" })
		{
			var list = new FieldList { Name = name }; doc.RootDictionary.Add(list); list.Fields.Add(field);
		}
		verify(doc, new[] { field }, write(doc));
		Assert.Same(doc.RootDictionary, field.Owner);
	}

	[Fact]
	public void PreparesListsInAnOrphanFieldsExtensionDictionary()
	{
		var doc = new CadDocument(); var outer = new FieldList { Name = "PORT_FIELDS" }; doc.RootDictionary.Add(outer);
		var parent = new Field { FormatString = string.Empty, XDictionary = new CadDictionary() }; outer.Fields.Add(parent);
		var inner = new FieldList { Name = "NESTED" }; parent.XDictionary.Add(inner);
		var field = new Field { FormatString = string.Empty }; inner.Fields.Add(field);
		verify(doc, new[] { parent, field }, write(doc));
		Assert.Same(inner, field.Owner);
	}
}
