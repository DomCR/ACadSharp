using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Objects;

public class XRecordTests
{
	/// <summary>
	/// The entries of an XRecord are positional: AutoCAD reads them in the order they were written.
	/// A handle entry has to keep the place it had in the file instead of being appended at the end,
	/// otherwise a record such as ADSK_XREC_VTRVIEWINFO becomes unreadable and AutoCAD refuses to
	/// open the drawing.
	/// </summary>
	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1032)]
	public void DwgRoundTripKeepsTheEntryOrderWithHandles(ACadVersion version)
	{
		CadDocument doc = new CadDocument(version);
		Layer layer = doc.Layers[Layer.DefaultName];

		XRecord xrecord = new XRecord("MOREDWG_ORDER");
		xrecord.CreateEntry(300, "before");
		xrecord.CreateEntry(330, layer);
		xrecord.CreateEntry(294, false);
		xrecord.CreateEntry(90, 7);

		CadDictionary dictionary = new CadDictionary("MOREDWG_TEST");
		dictionary.Add(xrecord);
		doc.RootDictionary.Add(dictionary);

		MemoryStream ms = new MemoryStream();
		DwgWriter.Write(ms, doc);
		using MemoryStream readStream = new MemoryStream(ms.ToArray());
		CadDocument rt = DwgReader.Read(readStream);

		Assert.True(rt.RootDictionary.TryGetEntry("MOREDWG_TEST", out CadDictionary rtDictionary));
		Assert.True(rtDictionary.TryGetEntry("MOREDWG_ORDER", out XRecord result));

		int[] codes = result.Entries.Select(e => e.Code).ToArray();
		Assert.Equal(new[] { 300, 330, 294, 90 }, codes);

		Assert.Equal("before", result.Entries.ElementAt(0).Value);
		Assert.Equal(Layer.DefaultName, ((Layer)result.Entries.ElementAt(1).Value).Name);
		Assert.False(System.Convert.ToBoolean(result.Entries.ElementAt(2).Value));
		Assert.Equal(7, System.Convert.ToInt32(result.Entries.ElementAt(3).Value));
	}

	/// <summary>
	/// Reading a drawing written by AutoCAD must give the same entry order the file has; this
	/// record has its handle in the middle (300, 302, 330, 294).
	/// </summary>
	[Fact]
	public void ReadsTheEntriesOfAnAutoCadFileInFileOrder()
	{
		string path = Path.Combine(TestVariables.SamplesFolder, "sample_AC1032.dwg");
		CadDocument doc = DwgReader.Read(path);

		XRecord record = null;
		foreach (View view in doc.Views)
		{
			if (view.XDictionary != null && view.XDictionary.TryGetEntry("ADSK_XREC_VTRVIEWINFO", out XRecord found))
			{
				record = found;
				break;
			}
		}

		Assert.NotNull(record);
		Assert.Equal(new[] { 300, 302, 330, 294 }, record.Entries.Select(e => e.Code).ToArray());
	}
}
