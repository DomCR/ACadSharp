using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class MTextTests : CommonEntityTests<MText>
	{
		[Fact]
		public void PlainTextTest()
		{
			MText text = new MText("- Color text {\\\\C3;green}, {\\\\C5;blue}, {\\\\C1;red}");

			//Hello this is an mText:\n
			//- Color text {\\C3;green}, {\\C5;blue}, {\\C1;red}, ByLayer, {\\C0;ByBlock}, {\\C21;\\c5872631;TrueColor\n}
			//- Font: {\\fCalibri|b0|i0|c0|p34;Calibri\\Fcdm|c0; CDM \\fConsolas|b0|i0|c0|p49;Consolas\\P}
			//- {\\H2x;Double height\\P}Hello this is an mText:"
		}
	}
}