using ACadSharp.Objects;
using System;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Objects;

public class CadDictionaryKeyTests
{
	[Fact]
	public void CloneKeepsTheEntryKeys()
	{
		//The dictionary stores each entry under its KEY, but Clone() rebuilt it from the objects' names: an entry
		//whose name differs from the key changed key, and an entry without a name threw.
		CadDictionary dictionary = new CadDictionary("RECORDS");
		dictionary.Add("KEY", new XRecord("ANOTHER-NAME"));
		dictionary.Add("NO-NAME", new XRecord());

		CadDictionary clone = (CadDictionary)dictionary.Clone();

		Assert.Equal(new[] { "KEY", "NO-NAME" }, clone.EntryNames.OrderBy(n => n, StringComparer.Ordinal));
		//and the lookup stays case-insensitive, like in the original collection
		Assert.True(clone.TryGetEntry<XRecord>("key", out _));
	}

	[Fact]
	public void RenamingAnObjectOnlyRekeysTheEntryStoredUnderItsName()
	{
		CadDictionary dictionary = new CadDictionary("RECORDS");
		XRecord byKey = new XRecord("NAME");
		dictionary.Add("KEY", byKey);
		XRecord byName = new XRecord("OLD");
		dictionary.Add(byName);

		byKey.Name = "OTHER";   //used to throw KeyNotFoundException (the entry was looked up as "NAME")
		byName.Name = "NEW";

		Assert.Equal(new[] { "KEY", "NEW" }, dictionary.EntryNames.OrderBy(n => n, StringComparer.Ordinal));
		Assert.Same(byKey, dictionary["KEY"]);
	}
}
