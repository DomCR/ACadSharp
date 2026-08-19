using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DXF
{
	/// <summary>
	/// The DWG writer says plainly that an AC1014 file is not accepted by AutoCAD. The DXF writer
	/// wrote the same version in silence, and the result is refused just as firmly: AutoCAD 2027
	/// reports "Premature end of object" on the DIMSTYLE table header, and removing that group only
	/// moves the refusal to the next object. A caller asking for that version gets a file that
	/// cannot be opened, so it has to be told.
	/// </summary>
	public class DxfVersionWarningTests
	{
		[Theory]
		[InlineData(ACadVersion.AC1014)]
		public void OldVersionIsReportedAsNotAcceptedByAutoCad(ACadVersion version)
		{
			List<string> messages = write(version);

			Assert.Contains(messages, m => m.Contains("not accepted by AutoCAD", System.StringComparison.OrdinalIgnoreCase));
		}

		[Theory]
		[InlineData(ACadVersion.AC1015)]
		[InlineData(ACadVersion.AC1018)]
		[InlineData(ACadVersion.AC1032)]
		public void SupportedVersionIsNotReported(ACadVersion version)
		{
			List<string> messages = write(version);

			Assert.DoesNotContain(messages, m => m.Contains("not accepted by AutoCAD", System.StringComparison.OrdinalIgnoreCase));
		}

		private static List<string> write(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;
			doc.ModelSpace.Entities.Add(new Line(new XYZ(0, 0, 0), new XYZ(10, 0, 0)));

			List<string> messages = new List<string>();
			using MemoryStream stream = new MemoryStream();
			using (DxfWriter writer = new DxfWriter(stream, doc, false))
			{
				writer.OnNotification += (s, e) => messages.Add(e.Message);
				writer.Write();
			}

			return messages;
		}
	}
}
