using ACadSharp.Classes;
using ACadSharp.DataStorage;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.Internal;

public class CadDocumentBuilderTests
{
	[Fact]
	public void BuildDataStorageSkipsOtherEntities()
	{
		CadDocument doc = new CadDocument();

		Solid3D solid = new Solid3D();
		doc.Entities.Add(solid);

		UnknownEntity surface = new UnknownEntity(new DxfClass { DxfName = "REVOLVEDSURFACE", CppClassName = "AcDbRevolvedSurface" });
		doc.Entities.Add(surface);

		byte[] solidData = new byte[] { 1, 2, 3 };

		DwgDocumentBuilder builder = new DwgDocumentBuilder(ACadVersion.AC1032, doc, new DwgReaderConfiguration { KeepUnknownEntities = true });
		builder.DataStorage = new CadFileDataStorage();
		builder.DataStorage.Records.Add(this.createAsmRecord(solid.Handle, solidData));
		builder.DataStorage.Records.Add(this.createAsmRecord(surface.Handle, new byte[] { 4, 5, 6 }));

		List<NotificationEventArgs> notifications = new List<NotificationEventArgs>();
		builder.OnNotification += (sender, e) => notifications.Add(e);

		builder.BuildDataStorage();

		Assert.Equal(solidData, solid.AcisData);
		NotificationEventArgs warning = Assert.Single(notifications);
		Assert.Equal(NotificationType.Warning, warning.NotificationType);
	}

	private AcdsRecord createAsmRecord(ulong handle, byte[] data)
	{
		AcdsRecord record = new AcdsRecord();

		record.Columns.Add(CadFileDataStorage.Id, new AcdsRecordColumn { Name = CadFileDataStorage.Id, Handle = handle });

		MemoryStream stream = new MemoryStream();
		stream.Write(data, 0, data.Length);
		record.Columns.Add(CadFileDataStorage.AsmData, new AcdsRecordColumn { Name = CadFileDataStorage.AsmData, Data = stream });

		return record;
	}
}
