using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using ACadSharp.Tests.TestModels;
using ACadSharp.XData;
using CSMath;
using System;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

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
						//*Registered Application Name: 3REAL_APP
						//* Code 1002, Starting or ending brace: {
						//*Code 1010, 3 real numbers: (19.5437 22.4009 0.0000)
						//*Code 1002, Starting or ending brace: }
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
					default:
						break;
				}
			}
		}
	}
}
