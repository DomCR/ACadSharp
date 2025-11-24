using ACadSharp.Entities;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class MTextTests : CommonEntityTests<MText>
	{
		[Fact]
		public void PlainTextTest()
		{
			MText text = new MText("- Font: {\\fCalibri|b0|i0|c0|p34;Calibri\\Fcdm|c0; CDM \\fConsolas|b0|i0|c0|p49;Consolas\\P}");
			Assert.Equal($"- Font: Calibri CDM Consolas{Environment.NewLine}", text.PlainText);

			text = new MText("- Color text {\\C3;green}, {\\C5;blue}, {\\C1;red}, ByLayer, {\\C0;ByBlock}, {\\C21;\\c5872631;TrueColor}");
			Assert.Equal("- Color text green, blue, red, ByLayer, ByBlock, TrueColor", text.PlainText);

			//Hello this is an mText:\n
			//- Color text {\\C3;green}, {\\C5;blue}, {\\C1;red}, ByLayer, {\\C0;ByBlock}, {\\C21;\\c5872631;TrueColor\n}
			//- Font: {\\fCalibri|b0|i0|c0|p34;Calibri\\Fcdm|c0; CDM \\fConsolas|b0|i0|c0|p49;Consolas\\P}
			//- {\\H2x;Double height\\P}Hello this is an mText:"

			//Hello this is an mText:\n- Color text {\\C3;green}, {\\C5;blue}, {\\C1;red}, ByLayer, {\\C0;ByBlock}, {\\C21;\\c5872631;TrueColor\n}- Font: {\\fCalibri|b0|i0|c0|p34;Text in Calibri\\Fcdm|c0; Text in CDM \\fConsolas|b0|i0|c0|p49;Text in Consolas\\P}- {\\H2x;Double height \\H0.875x;height is: 0.35\\H1.14286x;\\P}Hello this is an mText:\\P- {\\C4;Text in \\fCalibri|b0|i0|c0|p34;Calibri and cyan\\P\\pxqc;}Text in the center\\P\\pq*;Hello this is an mText:\\P{\\fArial|b1|i0|c0|p34;Bold Text \\fArial|b0|i1|c0|p34;Italic Text \\P}Special characters : \\{ \\} * \\\\ ~ % & ( ) ? ¿ ! ¡ 
			//Hello this is an mText:\n
			//- Color text {\\C3;green}, {\\C5;blue}, {\\C1;red}, ByLayer, {\\C0;ByBlock}, {\\C21;\\c5872631;TrueColor\n}
			//- Font: {\\fCalibri|b0|i0|c0|p34;Text in Calibri\\Fcdm|c0; Text in CDM \\fConsolas|b0|i0|c0|p49;Text in Consolas\\P}
			//- {\\H2x;Double height \\H0.875x;height is: 0.35\\H1.14286x;\\P}
			//Hello this is an mText:\\P
			//- {\\C4;Text in \\fCalibri|b0|i0|c0|p34;Calibri and cyan\\P
			//\\pxqc;}Text in the center\\P
			//\\pq*;Hello this is an mText:\\P
			//{\\fArial|b1|i0|c0|p34;Bold Text \\fArial|b0|i1|c0|p34;Italic Text \\P}
			//Special characters : \\{ \\} * \\\\ ~ % & ( ) ? ¿ ! ¡ 
		}
	}
}