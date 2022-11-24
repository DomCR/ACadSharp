using ACadSharp;
using ACadSharp.IO.DWG;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharpInternal.Tests
{
	public class DwgHandleWriterTests : DwgSectionWriterTestBase
	{
		public DwgHandleWriterTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgVersions))]
		public void WriteTest(ACadVersion version)
		{
			MemoryStream stream = new MemoryStream();
			var imap = this.generateMap();

			DwgHandleWriter writer = new DwgHandleWriter(version, stream, imap);
			writer.Write();

			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(version, stream, true);
			DwgHandleReader reader = new DwgHandleReader(sreader, version);
			reader.OnNotification += onNotification;

			var outmap = reader.Read();

			foreach (KeyValuePair<ulong, long> item in outmap)
			{
				Assert.True(imap.ContainsKey(item.Key));
				Assert.Equal(item.Value, imap[item.Key]);
				Assert.Equal(0, 0);
			}
		}

		private Dictionary<ulong, long> generateMap()
		{
			Dictionary<ulong, long> map = new Dictionary<ulong, long>();

			//Copy from the object writer test
			map.Add(9, 0);
			map.Add(14, 270);
			map.Add(2, 551);
			map.Add(18, 821);
			map.Add(5, 1099);
			map.Add(15, 1369);
			map.Add(16, 1657);
			map.Add(17, 1945);
			map.Add(4, 2239);
			map.Add(19, 2509);
			map.Add(7, 2819);
			map.Add(6, 3088);
			map.Add(1, 3357);
			map.Add(24, 3626);
			map.Add(22, 3926);
			map.Add(23, 4227);
			map.Add(27, 4499);
			map.Add(25, 4799);
			map.Add(26, 5100);

			return map;
		}
	}
}
