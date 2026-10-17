using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Entities.Tangent;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.Tangent
{
	/// <summary>
	/// Tests against payloads captured from a drawing that contains native Tianzheng objects
	/// (<c>TZ.dwg</c>, contributed in issue #1131). Each fixture is the class specific data of one
	/// object, taken byte for byte from the object as it was read.
	/// </summary>
	public class TchTextTests
	{
		private const string TextPayload =
		"ChGQVQWQOQOBBEwC9uR0z13ECPgOLDkLIeNASZ7AEYAOABGADAARgAxADIARgAtADkAMwA4ADkALQA0ADkARgBDAC0AOAAzA" +
		"DUARAAtADgAQwA4ADYAQwA2AEUARQBFAEUARABCAH0ASzhAOgA4QDmAOEA5wDhAOUA4QDlAOAA6QDhAOUA4QDlAOEA5QDhAO" +
		"UA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOkA4QDlAOAA+ADgAOkA4QDlAOE" +
		"A5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEG4QDmAOEA5QDhAOQA" +
		"RhUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBBVQAMgAwAGQAMQBBVQAMgAwAGQAMgB" +
		"BVQAMgAwAGQAMwBBVQAMgAwAGQANABFlQARABCAFgARABBAFQAQQBNAEEAUABfAEIATABPAEMASwBfAEUATgBEAF8ASZ7AEI" +
		"AMAA3ADEAMwBFADYAQgAtAEEARgBGADYALQA0ADgANwAxAC0AQQAyADcAMwAtADcAOAA5ADYAOAAzADQARAA2ADYANwA2AH0" +
		"AQbhAOUA4QDlAOEA5gBFFcAYQBsAGwAIAAmACAAVwBpAG4AZABvAHcAIAAmACAARABvAG8AcgBKOEA6ADhAOQA4QDmAOAA6Q" +
		"DhAOoA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOYA4QD" +
		"lAOEA5QDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QD" +
		"lAEG4QDpAOEA6ADhAOYAQbhAOMA4QDlAOEA4wCKFqkBRSiIoCB/f";

		private const string SquareColumnFixture =
		"ChGQVQWQOQOBBEbBi4iLTH3ECBSlmpfmkOFASZ7AEYAOABGADAARgAxADIARgAtADkAMwA4ADkALQA0ADkARgBDAC0AOAAzA" +
		"DUARAAtADgAQwA4ADYAQwA2AEUARQBFAEUARABCAH0ASzhAOUA4QDmAOEA5ADhAOUA4QDlAOAA6QDhAOUA4QDlAOEA5QDhAO" +
		"UA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOkA4QDlAOAA+ADgAOkA4QDlAOE" +
		"A5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEG4AD4AOEA5QDgAPcA" +
		"RhUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBBVQAMgAwAGQAMQBBVQAMgAwAGQAMgB" +
		"BVQAMgAwAGQAMwBBVQAMgAwAGQANABFlQARABCAFgARABBAFQAQQBNAEEAUABfAEIATABPAEMASwBfAEUATgBEAF8ASZ7AEI" +
		"AMAA3ADEAMwBFADYAQgAtAEEARgBGADYALQA0ADgANwAxAC0AQQAyADcAMwAtADcAOAA5ADYAOAAzADQARAA2ADYANwA2AH0" +
		"AQbgAPcA4QDlAOEA6gBDVMAcQB1AGEAcgBlACAAQwBvAGwAdQBtAG4ASjhAOkA4QDkAOAA9wDgAOkA4QDqAOEA5QDhAOUA4Q" +
		"DlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEo4AD3AOEA5QDhAOoA4ADpAOEA5QD" +
		"hAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QBBuEA4wDhAOgA4QDk" +
		"AEG4QDlAOEA5QDhAOcAGhapAUUoiKAgf7w==";

		private const string MTextFixture =
		"ChGQVQWQOQOBBEhF+0xNYe1kCEJmZ7s+5OVAAhFJnsARABGADcAMAA4AEIAMAAxAC0ARQBGADkAMAAtADQAOQA5ADIALQBCA" +
		"DgAQwA1AC0AMgBCADgANgBBADcARgA5AEQANQAxADYAfQBLOEA5gDhAOYA4QDqAOEA5QDhAOUA4ADpAOEA5QDhAOUA4QDlAO" +
		"EA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QBKOEA4wDhAOUA4QDqAOAA6QDhAOU" +
		"A4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUAQbgAPgA4QDlAOEA" +
		"4wBGFQARABCAFgARABBAFQAQQBNAEEAUABfAEIATABPAEMASwBfAEIARQBHAEkATgBfAEFVAAyADAAZAAxAEFVAAyADAAZAA" +
		"yAEFVAAyADAAZAAzAEFVAAyADAAZAA0AEWVABEAEIAWABEAEEAVABBAE0AQQBQAF8AQgBMAE8AQwBLAF8ARQBOAEQAXwBJns" +
		"AQgAwADcAMQAzAEUANgBCAC0AQQBGAEYANgAtADQAOAA3ADEALQBBADIANwAzAC0ANwA4ADkANgA4ADMANABEADYANgA3ADY" +
		"AfQBBuEA5ADhAOUA4QDpAErTQB1AGwAdABpAGwAaQBuAGUAIABUAGUAeAB0AA0ACgBMAGkAbgBlACAAMgANAAoATABpAG4AZ" +
		"QAgADMADQAKAC4ALgAuAA0ACgBMAGkAbgBlACAATgBKOEA4wDhAOQA4QDqAOAA6QDhAOoA4QDlAOEA5QDhAOUA4QDlAOEA5Q" +
		"DhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjgAPgA4QDlAOEA6ADgAOkA4QDlAOEA5QDhAOUA4QD" +
		"lAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEI4QDpAOEA5gDhAOQA4QDlAEG4QDpA" +
		"OEA5QDhAOgASzhAOQA4QDjAOEA6gDhAOUA4QDlAOAA6QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAO" +
		"UA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOoA4QDmAOEA5wDgAOkA4QDpAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOE" +
		"A5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDmAG4dqQFFKIigIHF";

		private const string DrawingNameFixture =
		"ChGQQRqD65+LEYs81AgCkSsDO6PkQEmewBGADgARgAwAEYAMQAyAEYALQA5ADMAOAA5AC0ANAA5AEYAQwAtADgAMwA1AEQAL" +
		"QA4AEMAOAA2AEMANgBFAEUARQBFAEQAQgB9AEs4AD4AOEA5gDhAOcA4QDlAOEA5QDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5Q" +
		"DhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEo4QDqAOEA5QDhAOMA4ADpAOEA5QDhAOUA4QD" +
		"lAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QBBuAA9wDhAOUA4QDqAEYVABEA" +
		"EIAWABEAEEAVABBAE0AQQBQAF8AQgBMAE8AQwBLAF8AQgBFAEcASQBOAF8ARZUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAE" +
		"wATwBDAEsAXwBFAE4ARABfAEmewBCADAANwAxADMARQA2AEIALQBBAEYARgA2AC0ANAA4ADcAMQAtAEEAMgA3ADMALQA3ADg" +
		"AOQA2ADgAMwA0AEQANgA2ADcANgB9AEG4AD4AOEA5QDhAOQAQ1EAHIAYQB3AGkAbgBnACAAVABpAHQAbABlAEFMQA6ADEAMA" +
		"AwAEo4QDoAOEA5QDhAOgA4ADpAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QD" +
		"hAOUA4QDlAOEA5QBKOEA5wDhAOgA4QDoAOAA6QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDl" +
		"AOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOcA4QDqAOEA4wDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhA" +
		"OUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEo4QDlAOEA5QDgAPgA4ADpAOEA6ADhAOUA4QDlAOEA5QDhAOUA4QDlAO" +
		"EA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QBKOEA5gDhAOUA4QDqAOAA6QDhAOcA4QDlAOEA5QDhAOU" +
		"A4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUAQbhAOcA4QDlAOEA4wBBuAA+ADhAOYA4" +
		"QDmAEG4QDqAOEA6ADhAOcAFB2pAUQoiKiI/L";

		private static byte[] decode(string base64)
		{
			return Convert.FromBase64String(base64);
		}

		private static DxfClass tchClass(string dxfName)
		{
			return new DxfClass
			{
				DxfName = dxfName,
				CppClassName = "TDb" + dxfName.Substring(4),
				ApplicationName = "TCH_KERNAL",
			};
		}

		private static TchText read(string dxfName, string fixture)
		{
			byte[] payload = decode(fixture);
			TchText text = new TchText(tchClass(dxfName))
			{
				//The reader fills these from the common entity data, which a fixture does not
				//carry; a conversion copies them onto what it builds, so they have to be here.
				Layer = new ACadSharp.Tables.Layer("PUB_TEXT"),
				LineType = ACadSharp.Tables.LineType.ByLayer,
			};

			TchTextReader.Read(text, payload, payload.Length * 8L);

			return text;
		}

		[Fact]
		public void TchTextReadsThePositionTheExportDrawsItAt()
		{
			TchText text = read("TCH_TEXT", TextPayload);

			Assert.True(text.HasPosition);
			Assert.Equal(39176.3507, text.InsertionPoint.X, 4);
			Assert.Equal(29653.1919, text.InsertionPoint.Y, 4);
		}

		[Fact]
		public void TchTextReadsTheHeightItIsDrawnAt()
		{
			//The value after the text is the height in the drawing's scale, and the scale is the
			//second string of the stream. This object carries 3.5 and the drawing is at 1:100, which
			//is the 350 the export draws it at.
			TchText text = read("TCH_TEXT", TextPayload);

			Assert.Equal(350.0, text.Height, 6);
			Assert.Equal(350.0, text.ToTextEntity().Height, 6);
		}

		[Fact]
		public void TchTextReadsTheContent()
		{
			TchText text = read("TCH_TEXT", TextPayload);

			//The export splits this into per-word text entities, but the object stores it whole.
			Assert.Equal("Wall & Window & Door", text.Value);
		}

		[Fact]
		public void TchTextReadsContentThatLooksLikeNothingElse()
		{
			Assert.Equal("Square Column", read("TCH_TEXT", SquareColumnFixture).Value);
		}

		[Fact]
		public void TchTextReadsTheContentOfAMultilineText()
		{
			//The export splits this into one entity per word per line; the object stores it whole.
			//The line breaks are the reason this read as nothing for a while: a filter that allowed
			//only printable characters rejected every multiline text while every single line one
			//still came through.
			TchText text = read("TCH_MTEXT", MTextFixture);

			Assert.Equal("Multiline Text\r\nLine 2\r\nLine 3\r\n...\r\nLine N", text.Value);
		}

		[Fact]
		public void TchTextReadsTheNameOfATitleBlock()
		{
			Assert.Equal("Drawing Title", read("TCH_DRAWINGNAME", DrawingNameFixture).Value);
		}

		[Fact]
		public void TchTextIsNotATextEntityButConvertsToThem()
		{
			TchText text = read("TCH_TEXT", TextPayload);

			Assert.IsNotType<TextEntity>(text);

			TextEntity converted = text.Explode().OfType<TextEntity>().Single();

			Assert.Equal(text.Value, converted.Value);
			Assert.Equal(text.InsertionPoint.X, converted.InsertPoint.X, 9);
			Assert.Equal(text.InsertionPoint.Y, converted.InsertPoint.Y, 9);
		}

		[Fact]
		public void TchTextCarriesTheClassItWasReadFrom()
		{
			Assert.Equal("TCH_MTEXT", read("TCH_MTEXT", MTextFixture).DxfClass.DxfName);
		}

		[Fact]
		public void TchTextMovesWithTheTransform()
		{
			TchText text = read("TCH_TEXT", TextPayload);

			XYZ before = text.InsertionPoint;

			text.ApplyTransform(Transform.CreateTranslation(new XYZ(10.0, 20.0, 0.0)));

			Assert.Equal(before.X + 10.0, text.InsertionPoint.X, 9);
			Assert.Equal(before.Y + 20.0, text.InsertionPoint.Y, 9);
		}

		[Fact]
		public void TchTextRejectsAPayloadThatIsTooShort()
		{
			TchText text = new TchText(tchClass("TCH_TEXT"));

			TchTextReader.Read(text, new byte[8], 64L);

			Assert.False(text.HasPosition);
			Assert.Null(text.Value);
		}

		[Fact]
		public void TchTextTakesNothingFromANullPayload()
		{
			TchText text = new TchText(tchClass("TCH_TEXT"));

			TchTextReader.Read(text, null, 0L);

			Assert.False(text.HasPosition);
		}

		[Fact]
		public void TchObjectReaderFindsTheStringStream()
		{
			//The stream is described from its end, and the search only accepts a candidate whose
			//strings fill the space up to the size field exactly. On this object that lands on bit
			//216, which is where the first string begins.
			byte[] payload = decode(TextPayload);

			Assert.Equal(216L, TchObjectReader.FindStringStream(payload, payload.Length * 8L));
		}

		[Fact]
		public void TchObjectReaderReadsTheScaleEveryObjectCarries()
		{
			//The second string of the stream, after the class identifier, is the drawing's scale,
			//and it is what turns the multiples a class records into lengths.
			byte[] payload = decode(TextPayload);
			long stream = TchObjectReader.FindStringStream(payload, payload.Length * 8L);

			double scale = TchObjectReader.ReadStreamedDouble(
				TchObjectReader.SkipString(new TchStreamReader(payload, stream)), TgDataEncrypt.ObjectXor);

			Assert.Equal(100.0, scale, 9);
		}
	}
}
