using ACadSharp.IO;
using ACadSharp.Tables;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

/// <summary>
/// Tests for malformed files whose BLOCK_CONTROL object references the reserved
/// *Model_Space/*Paper_Space block records more than once, or not at all.
/// Such files are produced by some third-party DXF->DWG converters and must be
/// read without throwing, surfacing the anomaly as a notification.
/// </summary>
public class DwgReaderMalformedBlockCtrlTests
{
	private const string _dataFolder = "../../../../ACadSharp.Tests/Data/";

	public static string DupPaperSpacePath { get { return Path.Combine(_dataFolder, "dup_paperspace_r2000.dwg"); } }

	public static string MissingModelSpacePath { get { return Path.Combine(_dataFolder, "missing_modelspace_r2000.dwg"); } }

	[Fact]
	public void ReadFileWithDuplicatePaperSpace()
	{
		List<NotificationEventArgs> notifications = new();
		CadDocument doc = null;

		var ex = Record.Exception(() =>
		{
			doc = DwgReader.Read(DupPaperSpacePath, (s, e) => notifications.Add(e));
		});

		Assert.Null(ex);
		Assert.NotNull(doc);
		Assert.NotNull(doc.ModelSpace);
		Assert.NotNull(doc.PaperSpace);
		Assert.NotEmpty(doc.Entities.ToList());

		//The reserved records must be registered exactly once
		Assert.Equal(1, doc.BlockRecords.Count(b => b.Name.Equals(BlockRecord.ModelSpaceName, System.StringComparison.OrdinalIgnoreCase)));
		Assert.Equal(1, doc.BlockRecords.Count(b => b.Name.Equals(BlockRecord.PaperSpaceName, System.StringComparison.OrdinalIgnoreCase)));

		//The anomaly must be surfaced as a notification, not swallowed
		Assert.Contains(notifications, n =>
			(n.NotificationType == NotificationType.Error || n.NotificationType == NotificationType.Warning)
			&& n.Message.Contains(BlockRecord.PaperSpaceName));
	}

	[Fact]
	public void ReadFileWithMissingModelSpace()
	{
		List<NotificationEventArgs> notifications = new();
		CadDocument doc = null;

		var ex = Record.Exception(() =>
		{
			doc = DwgReader.Read(MissingModelSpacePath, (s, e) => notifications.Add(e));
		});

		Assert.Null(ex);
		Assert.NotNull(doc);
		Assert.NotNull(doc.ModelSpace);
		Assert.NotNull(doc.PaperSpace);

		//Enumeration of the entities must not throw
		var entities = doc.Entities.ToList();

		Assert.Equal(1, doc.BlockRecords.Count(b => b.Name.Equals(BlockRecord.ModelSpaceName, System.StringComparison.OrdinalIgnoreCase)));
		Assert.Equal(1, doc.BlockRecords.Count(b => b.Name.Equals(BlockRecord.PaperSpaceName, System.StringComparison.OrdinalIgnoreCase)));

		Assert.Contains(notifications, n =>
			(n.NotificationType == NotificationType.Error || n.NotificationType == NotificationType.Warning)
			&& n.Message.Contains(BlockRecord.ModelSpaceName));
	}

	[Fact]
	public void ReadWellFormedFileKeepsSingleReservedRecords()
	{
		string path = Path.Combine(TestVariables.SamplesFolder, "sample_AC1015.dwg");

		List<NotificationEventArgs> notifications = new();
		CadDocument doc = DwgReader.Read(path, (s, e) => notifications.Add(e));

		Assert.NotNull(doc.ModelSpace);
		Assert.NotNull(doc.PaperSpace);

		Assert.Equal(1, doc.BlockRecords.Count(b => b.Name.Equals(BlockRecord.ModelSpaceName, System.StringComparison.OrdinalIgnoreCase)));
		Assert.Equal(1, doc.BlockRecords.Count(b => b.Name.Equals(BlockRecord.PaperSpaceName, System.StringComparison.OrdinalIgnoreCase)));

		//A well-formed file must not report anomalies for the reserved records
		Assert.DoesNotContain(notifications, n => n.Message.Contains("reserved block record"));
		Assert.DoesNotContain(notifications, n => n.Message.Contains("block record is missing"));
	}
}
