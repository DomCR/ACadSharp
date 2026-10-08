using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.Tangent;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.Tangent
{
	/// <summary>
	/// Tests for <see cref="TchDataMap"/> against real Tianzheng payloads.
	/// </summary>
	/// <remarks>
	/// The fixtures are retained custom-object payloads taken from real proxy entities of a Tianzheng
	/// plumbing drawing (class id 575 == TCH_PIPE, 576 == TCH_PIPEFITTING). They cover both cases:
	/// objects that carry properties and objects whose property map is empty.
	/// </remarks>
	public class TchDataMapTests
	{
		private readonly ITestOutputHelper _output;

		public TchDataMapTests(ITestOutputHelper output)
		{
			this._output = output;
		}

		[Theory]
		[InlineData(PayloadPipeA, "PIPE_SYSTEM_NAME", "\u6d88\u9632")]
		[InlineData(PayloadPipeA, "LINKED_DIM_TEXT", "DN25")]
		[InlineData(PayloadPipeA, "PIPE_OUTER_DN", null)]
		[InlineData(PayloadPipeA, "PIPE_THICK", null)]
		public void ReadsNamedPropertiesFromRealPayload(string base64, string name, string expected)
		{
			TchDataMap map = read(base64);

			this._output.WriteLine(string.Join(" | ", map.Entries));
			Assert.True(map.Properties.ContainsKey(name), "missing property " + name);

			if (expected != null)
			{
				Assert.Equal(expected, map.Properties[name]);
			}
		}

		[Fact]
		public void EmptyPropertyMapIsReportedAsEmpty()
		{
			// class id 576 == TCH_PIPEFITTING carries no properties at all
			TchDataMap map = read(PayloadFitting);

			Assert.True(map.IsEmpty);
			Assert.Empty(map.Properties);
			Assert.True(map.IsComplete);
			Assert.Equal(2, map.Entries.Count);
			Assert.Equal(
				new[] { TchDataMap.BlockBeginMarker, TchDataMap.BlockEndMarker },
				map.Entries);
		}

		[Fact]
		public void DecodesEveryFixtureAndReachesTheClosingMarker()
		{
			foreach (string payload in new[] { PayloadPipeA, PayloadPipeB, PayloadPipeC, PayloadFitting })
			{
				TchDataMap map = read(payload);
				Assert.NotNull(map);
				Assert.True(map.IsComplete, "closing marker not reached");
			}
		}

		[Fact]
		public void TruncatedPayloadStopsWithoutThrowing()
		{
			byte[] full = Convert.FromBase64String(PayloadPipeA);
			byte[] truncated = full.Take(full.Length / 2).ToArray();

			TchDataMap map = TchDataMap.Read(truncated, ACadVersion.AC1027);

			Assert.NotNull(map);
			Assert.False(map.IsComplete);
		}

		private static TchDataMap read(string base64)
		{
			return TchDataMap.Read(Convert.FromBase64String(base64), ACadVersion.AC1027);
		}

		private const string PayloadPipeA =
			"Q9MAEkATgBLAEUARABfAEQASQBNAF8AVABFAFgAVABBEQATgAyADUAQ1QAEkAUABFAF8ATwBVAFQARQBSAF8ARABOAEQUABJAFAA" +
			"RQBfAFMAWQBTAFQARQBNAF8ATgBBAE0ARQBAohtMpZClAASQBQAEUAXwBUAEgASQBDAEsARhUAEQAQgBYAEQAQQBUAEEATQBBAFA" +
			"AXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBFlQARABCAFgARABBAFQAQQBNAEEAUABfAEIATABPAEMASwBfAEUATgBEAF8AkCRA" +
			"BOAI";

		private const string PayloadPipeB =
			"Q9MAEkATgBLAEUARABfAEQASQBNAF8AVABFAFgAVABBEQATgAyADUAQ1QAEkAUABFAF8ATwBVAFQARQBSAF8ARABOAEQUABJAFAA" +
			"RQBfAFMAWQBTAFQARQBNAF8ATgBBAE0ARQBAohtMpZClAASQBQAEUAXwBUAEgASQBDAEsARhUAEQAQgBYAEQAQQBUAEEATQBBAFA" +
			"AXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBFlQARABCAFgARABBAFQAQQBNAEEAUABfAEIATABPAEMASwBfAEUATgBEAF8AkCRA" +
			"BOAI";

		private const string PayloadPipeC =
			"Q9MAEkATgBLAEUARABfAEQASQBNAF8AVABFAFgAVABBEQATgAyADUAQ1QAEkAUABFAF8ATwBVAFQARQBSAF8ARABOAEQUABJAFAA" +
			"RQBfAFMAWQBTAFQARQBNAF8ATgBBAE0ARQBAohtMpZClAASQBQAEUAXwBUAEgASQBDAEsARhUAEQAQgBYAEQAQQBUAEEATQBBAFA" +
			"AXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBFlQARABCAFgARABBAFQAQQBNAEEAUABfAEIATABPAEMASwBfAEUATgBEAF8AkCRA" +
			"BOAI";

		private const string PayloadFitting =
			"RhUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBFlQARABCAFgARABBAFQAQQBNAEEAUABfAE" +
			"IATABPAEMASwBfAEUATgBEAF8AqM";

	}
}
