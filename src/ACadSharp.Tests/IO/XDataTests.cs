using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using ACadSharp.XData;
using CSMath;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class XDataTests : IOTestsBase
	{
		public static TheoryData<FileModel> Files { get; } = new();

		static XDataTests()
		{
			loadSamples("xdata", "*", Files);
		}

		public XDataTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(Files))]
		public void ReadXData(FileModel test)
		{
			CadDocument doc = this.readDocument(test);

			foreach (TextEntity e in doc.Entities.OfType<TextEntity>())
			{
				ExtendedData edata = null;
				ExtendedDataRecord record = null;

				switch (e.Value.ToLowerInvariant())
				{
					case "xdata 3real":
						Assert.True(e.ExtendedData.TryGet("3REAL_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataXCoordinate, record.Code);
						Assert.True(new XYZ(19.5437, 22.4009, 0.0000).Equals((XYZ)record.RawValue, 2));

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata dir":
						Assert.True(e.ExtendedData.TryGet("DIR_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataWorldXDir, record.Code);
						Assert.True(new XYZ(16.5903, -14.3765, 0.0000).Equals((XYZ)record.RawValue, 2));

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata disp":
						Assert.True(e.ExtendedData.TryGet("DISP_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataWorldXDisp, record.Code);
						Assert.True(new XYZ(32.2013, -11.9528, 0.0000).Equals((XYZ)record.RawValue, 2));

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata dist":
						Assert.True(e.ExtendedData.TryGet("DIST_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataDist, record.Code);
						Assert.Equal(41.0317, (double)record.RawValue, 0.001);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata hand":
						Assert.True(e.ExtendedData.TryGet("HANDLE_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataHandle, record.Code);
						Assert.Equal(0x10ul, (ulong)record.RawValue);

						var handle = record as ExtendedDataHandle;

						Assert.NotNull(handle.ResolveReference(doc));

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata int":
						Assert.True(e.ExtendedData.TryGet("INT_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataInteger16, record.Code);
						Assert.True(524 == (short)record.RawValue);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata layer":
						Assert.True(e.ExtendedData.TryGet("LAYER_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataLayerName, record.Code);

						ExtendedDataLayer layRecord = record as ExtendedDataLayer;
						Layer layer = layRecord.ResolveReference(doc);

						Assert.NotNull(layer);
						Assert.Equal("app_layer", layer.Name);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata long":
						Assert.True(e.ExtendedData.TryGet("APP_LONG", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataInteger32, record.Code);
						Assert.True(6654 == (int)record.RawValue);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata pos":
						Assert.True(e.ExtendedData.TryGet("POS_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataWorldXCoordinate, record.Code);
						Assert.True(new XYZ(52.3480, -11.6366, 0.0000).Equals((XYZ)record.RawValue, 2));

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata real":
						Assert.True(e.ExtendedData.TryGet("REAL_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataReal, record.Code);
						Assert.Equal(54.2540, (double)record.RawValue, 0.001);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata scale":
						Assert.True(e.ExtendedData.TryGet("SCALE_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataScale, record.Code);
						Assert.Equal(548.5400, (double)record.RawValue, 0.001);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata str":
						Assert.True(e.ExtendedData.TryGet("STR_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataAsciiString, record.Code);
						Assert.Equal("hello this is my string", (string)record.RawValue);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					case "xdata all":
						Assert.True(e.ExtendedData.TryGet("MY_APP", out edata));
						record = edata.Records[0];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('{', record.RawValue);

						record = edata.Records[1];
						Assert.Equal(DxfCode.ExtendedDataAsciiString, record.Code);
						Assert.Equal("hello this is a xdata record", (string)record.RawValue);

						record = edata.Records[2];
						Assert.Equal(DxfCode.ExtendedDataXCoordinate, record.Code);
						Assert.True(new XYZ(132.3687, 13.4163, 0.0000).Equals((XYZ)record.RawValue, 2));

						record = edata.Records[3];
						Assert.Equal(DxfCode.ExtendedDataControlString, record.Code);
						Assert.Equal('}', record.RawValue);
						break;
					default:
						throw new Exception();
				}
			}
		}
	}
}
