using ACadSharp.IO;
using ACadSharp.Tables;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfTablesSectionTests
{
	[Fact]
	public void ReadDuplicatedEntryWithoutDefaultEntry()
	{
		// LAYER table with the name A twice and without the layer 0: the duplicate
		// is found while the table is still being read, before the default entry exists.
		string dxf = layerTableDxf("A", "A");

		List<NotificationEventArgs> notifications = new();
		CadDocument doc = read(dxf, notifications);

		Assert.Contains(notifications, n => n.NotificationType == NotificationType.Warning && n.Message.Contains("Duplicated entry"));
		Assert.True(doc.Layers.Contains("A"));
		Assert.True(doc.Layers.Contains(Layer.DefaultName));
		Assert.Equal(2, doc.Layers.Count);
	}

	private static string layerTableDxf(params string[] names)
	{
		List<string> lines = new()
		{
			"0", "SECTION",
			"2", "TABLES",
			"0", "TABLE",
			"2", "LAYER",
			"70", names.Length.ToString(),
		};

		foreach (string name in names)
		{
			lines.AddRange(new[]
			{
				"0", "LAYER",
				"2", name,
				"70", "0",
				"62", "7",
				"6", "CONTINUOUS",
			});
		}

		lines.AddRange(new[]
		{
			"0", "ENDTAB",
			"0", "ENDSEC",
			"0", "SECTION",
			"2", "ENTITIES",
			"0", "ENDSEC",
			"0", "EOF",
		});

		return string.Join("\n", lines);
	}

	private static CadDocument read(string dxf, List<NotificationEventArgs> notifications)
	{
		using MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dxf));
		using DxfReader reader = new DxfReader(stream);
		reader.OnNotification += (sender, e) => notifications.Add(e);
		return reader.Read();
	}
}
