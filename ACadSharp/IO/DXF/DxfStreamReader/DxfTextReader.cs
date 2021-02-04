using ACadSharp.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfTextReader : StreamReader, IDxfStreamReader
	{
		public bool SectionEndFound { get; private set; } = false;
		public DxfCode LastDxfCode { get; private set; }
		public int LastCode { get { return (int)LastDxfCode; } }
		public object LastValue { get; private set; }
		public int Line { get; private set; }

		public string LastValueAsString { get { return LastValue.ToString(); } }
		public bool LastValueAsBool { get { return lineAsBool(LastValueAsString); } }
		public short LastValueAsShort { get { return lineAsShort(LastValueAsString); } }
		public int LastValueAsInt { get { return lineAsInt(LastValueAsString); } }
		public long LastValueAsLong { get { return lineAsLong(LastValueAsString); } }
		public double LastValueAsDouble { get { return lineAsDouble(LastValueAsString); } }
		public ulong LastValueAsHandle { get { return lineAsHandle(LastValueAsString); } }
		public byte[] LastValueAsBinaryChunk { get { return lineAsBinaryChunk(LastValueAsString); } }


		public DxfTextReader(Stream stream) : base(stream)
		{
			start();
		}
		public DxfTextReader(Stream stream, Encoding encoding) : base(stream, encoding)
		{
			start();
		}
		//**************************************************************************
		public void Find(string dxfEntry)
		{
			start();

			do
			{
				ReadNext();
			}
			while (LastValueAsString != dxfEntry && (LastValueAsString != DxfFileToken.EndOfFile));

			//Reset the end section flag
			SectionEndFound = false;
		}
		public Tuple<DxfCode, object> ReadNext()
		{
			LastDxfCode = readCode();
			LastValue = readValue((int)LastDxfCode);

			//Check for the end of the section
			if (LastValueAsString == DxfFileToken.EndSection)
				SectionEndFound = true;

			Tuple<DxfCode, object> pair = new Tuple<DxfCode, object>(LastDxfCode, LastValue);

			return pair;
		}
		public override string ReadLine()
		{
			Line++;
			return base.ReadLine();
		}
		//**************************************************************************
		private void start()
		{
			LastDxfCode = DxfCode.Invalid;
			LastValue = string.Empty;
			SectionEndFound = false;

			BaseStream.Position = 0;
			DiscardBufferedData();

			Line = 0;
		}
		private bool lineAsBool(string str)
		{
			if (byte.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte result))
			{
				return result > 0;
			}

			return false;
		}
		private double lineAsDouble(string str)
		{
			if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
			{
				return result;
			}

			return 0.0;
		}
		private short lineAsShort(string str)
		{
			if (short.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out short result))
			{
				return result;
			}

			return 0;
		}
		private int lineAsInt(string str)
		{
			if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
			{
				return result;
			}

			return 0;
		}
		private long lineAsLong(string str)
		{
			if (long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
			{
				return result;
			}

			return 0;
		}
		private ulong lineAsHandle(string str)
		{
			if (ulong.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong result))
			{
				return result;
			}

			return 0;
		}
		private byte[] lineAsBinaryChunk(string str)
		{
			byte[] bytes = new byte[str.Length];

			for (int i = 0; i < str.Length; i++)
			{
				//Create a byte value
				string hex = $"{str[i]}{str[++i]}";

				if (byte.TryParse(hex, NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out byte result))
				{
					bytes[i] = result;
				}
				else
				{
					return new byte[0];
				}
			}

			return bytes;
		}

		private DxfCode readCode()
		{
			string line = ReadLine();

			if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
			{
				return (DxfCode)value;
			}

			return DxfCode.Invalid;
		}
		private object readValue(int code)
		{
			string strVal = ReadLine();

			if (code >= 0 && code <= 9)
				return strVal;
			if (code >= 10 && code <= 39)
				return lineAsDouble(strVal);
			if (code >= 40 && code <= 59)
				return lineAsDouble(strVal);
			if (code >= 60 && code <= 79)
				return lineAsShort(strVal);
			if (code >= 90 && code <= 99)
				return lineAsInt(strVal);
			if (code == 100)
				return strVal;
			if (code == 101)
				return strVal;
			if (code == 102)
				return strVal;
			if (code == 105)
				return lineAsHandle(strVal);
			if (code >= 110 && code <= 119)
				return lineAsDouble(strVal);
			if (code >= 120 && code <= 129)
				return lineAsDouble(strVal);
			if (code >= 130 && code <= 139)
				return lineAsDouble(strVal);
			if (code >= 140 && code <= 149)
				return lineAsDouble(strVal);
			if (code >= 160 && code <= 169)
				return lineAsLong(strVal);
			if (code >= 170 && code <= 179)
				return lineAsShort(strVal);
			if (code >= 210 && code <= 239)
				return lineAsDouble(strVal);
			if (code >= 270 && code <= 279)
				return lineAsShort(strVal);
			if (code >= 280 && code <= 289)
				return lineAsShort(strVal);
			if (code >= 290 && code <= 299)
				return lineAsBool(strVal);
			if (code >= 300 && code <= 309)
				return strVal;
			if (code >= 310 && code <= 319)
				return lineAsBinaryChunk(strVal);
			if (code >= 320 && code <= 329)
				return lineAsHandle(strVal);
			if (code >= 330 && code <= 369)
				return lineAsHandle(strVal);
			if (code >= 370 && code <= 379)
				return lineAsShort(strVal);
			if (code >= 380 && code <= 389)
				return lineAsShort(strVal);
			if (code >= 390 && code <= 399)
				return lineAsHandle(strVal);
			if (code >= 400 && code <= 409)
				return lineAsShort(strVal);
			if (code >= 410 && code <= 419)
				return strVal;
			if (code >= 420 && code <= 429)
				return lineAsInt(strVal);
			if (code >= 430 && code <= 439)
				return strVal;
			if (code >= 440 && code <= 449)
				return lineAsInt(strVal);
			if (code >= 450 && code <= 459)
				return lineAsInt(strVal);
			if (code >= 460 && code <= 469)
				return lineAsDouble(strVal);
			if (code >= 470 && code <= 479)
				return strVal;
			if (code >= 480 && code <= 481)
				return lineAsHandle(strVal);
			if (code == 999)
				return strVal;
			if (code >= 1010 && code <= 1059)
				return lineAsDouble(strVal);
			if (code >= 1000 && code <= 1003)
				return strVal;
			if (code == 1004)
				return lineAsBinaryChunk(strVal);
			if (code >= 1005 && code <= 1009)
				return strVal;
			if (code >= 1060 && code <= 1070)
				return lineAsShort(strVal);
			if (code == 1071)
				return lineAsInt(strVal);

			throw new DxfException(code, Line);
		}

		[Obsolete("Not working, cannot handle udocumented codes")]
		private object readValue(DxfCode code)
		{
			string strVal = ReadLine();

			switch (code)
			{
				case DxfCode.XDictionary:
				case DxfCode.PReactors:
				case DxfCode.Operator:
				case DxfCode.XDataStart:
				case DxfCode.HeaderId:
				case DxfCode.End:
					return null;
				case DxfCode.Start:
				case DxfCode.Text:
				case DxfCode.ShapeName:
				case DxfCode.AttributePrompt:
				case DxfCode.TextBigFontFile:
				case DxfCode.Handle:
				case DxfCode.DimBlk1:
				case DxfCode.DimBlk2:
				case DxfCode.LayerName:
				case DxfCode.CLShapeText:
					return ReadLine();
				case DxfCode.XCoordinate:
				case DxfCode.YCoordinate:
				case DxfCode.ZCoordinate:
				case DxfCode.Elevation:
				case DxfCode.Thickness:
					return lineAsDouble(strVal);
				case DxfCode.Real:
				case DxfCode.TxtStyleXScale:
				case DxfCode.TxtStylePSize:
				case DxfCode.ViewFrontClip:
				case DxfCode.ViewBackClip:
				case DxfCode.ShapeYOffset:
				case DxfCode.ShapeScale:
				case DxfCode.PixelScale:
				case DxfCode.LinetypeScale:
				case DxfCode.DashLength:
				case DxfCode.Angle:
				case DxfCode.ViewportTwist:
					return lineAsDouble(strVal);  //float ??
				case DxfCode.Visibility:
				case DxfCode.LayerLinetype:
				case DxfCode.Color:
				case DxfCode.HasSubentities:
				case DxfCode.ViewportVisibility:
				case DxfCode.ViewportActive:
				case DxfCode.ViewportNumber:
				case DxfCode.Int16:
				case DxfCode.ViewMode:
				case DxfCode.CircleSides:
				case DxfCode.ViewportZoom:
				case DxfCode.ViewportIcon:
				case DxfCode.ViewportSnap:
				case DxfCode.ViewportGrid:
				case DxfCode.ViewportSnapStyle:
				case DxfCode.ViewportSnapPair:
					return lineAsShort(strVal);
				case DxfCode.Int32:
					return lineAsInt(strVal);
				case DxfCode.Subclass:
				case DxfCode.EmbeddedObjectStart:
				case DxfCode.ControlString:
					return LastValueAsString;
				case DxfCode.DimVarHandle:
					return lineAsHandle(strVal);
				case DxfCode.UcsOrg:
				case DxfCode.UcsOrientationX:
				case DxfCode.UcsOrientationY:
				case DxfCode.XReal:
				case DxfCode.ViewBrightness:
				case DxfCode.ViewContrast:
					return lineAsDouble(strVal);
				case DxfCode.Int64:
					return lineAsLong(strVal);
				case DxfCode.XInt16:
					return lineAsShort(strVal);
				case DxfCode.NormalX:
				case DxfCode.NormalY:
				case DxfCode.NormalZ:
					return lineAsDouble(strVal);
				case DxfCode.XXInt16:
				case DxfCode.Int8:
				case DxfCode.RenderMode:
					return lineAsShort(strVal);
				case DxfCode.Bool:
					return lineAsBool(strVal);
				case DxfCode.XTextString:
					return ReadLine();
				case DxfCode.BinaryChunk:
					return lineAsBinaryChunk(strVal);
				case DxfCode.ArbitraryHandle:
				case DxfCode.SoftPointerId:
				case DxfCode.HardPointerId:
				case DxfCode.MaterialHandleId:
				case DxfCode.SoftOwnershipId:
				case DxfCode.HardOwnershipId:
					return lineAsHandle(strVal);
				case DxfCode.LineWeight:
				case DxfCode.PlotStyleNameType:
					return lineAsShort(strVal);
				case DxfCode.PlotStyleNameId:
					return lineAsHandle(strVal);
				case DxfCode.ExtendedInt16:
					return lineAsShort(strVal);
				case DxfCode.LayoutName:
					return ReadLine();
				case DxfCode.ColorRgb:
					return lineAsInt(strVal);
				case DxfCode.ColorName:
					return ReadLine();
				case DxfCode.Alpha:
				case DxfCode.GradientObjType:
				case DxfCode.GradientPatType:
				case DxfCode.GradientTintType:
				case DxfCode.GradientColCount:
					return lineAsInt(strVal);
				case DxfCode.GradientAngle:
				case DxfCode.GradientShift:
				case DxfCode.GradientTintVal:
				case DxfCode.GradientColVal:
					return lineAsDouble(strVal);
				case DxfCode.GradientName:
					return ReadLine();
				case DxfCode.HardPointHandle:
				case DxfCode.HardPointHandle1:
					return lineAsHandle(strVal);
				case DxfCode.Comment:
					return ReadLine();
				case DxfCode.ExtendedDataAsciiString:
				case DxfCode.ExtendedDataRegAppName:
				case DxfCode.ExtendedDataControlString:
				case DxfCode.ExtendedDataLayerName:
					return ReadLine();
				case DxfCode.ExtendedDataBinaryChunk:
					return lineAsBinaryChunk(strVal);
				case DxfCode.ExtendedDataHandle:
					return ReadLine();
				case DxfCode.ExtendedDataXCoordinate:
				case DxfCode.ExtendedDataWorldXCoordinate:
				case DxfCode.ExtendedDataWorldXDisp:
				case DxfCode.ExtendedDataWorldXDir:
				case DxfCode.ExtendedDataYCoordinate:
				case DxfCode.ExtendedDataWorldYCoordinate:
				case DxfCode.ExtendedDataWorldYDisp:
				case DxfCode.ExtendedDataWorldYDir:
				case DxfCode.ExtendedDataZCoordinate:
				case DxfCode.ExtendedDataWorldZCoordinate:
				case DxfCode.ExtendedDataWorldZDisp:
				case DxfCode.ExtendedDataWorldZDir:
				case DxfCode.ExtendedDataReal:
				case DxfCode.ExtendedDataDist:
				case DxfCode.ExtendedDataScale:
					return lineAsDouble(strVal);
				case DxfCode.ExtendedDataInteger16:
					return lineAsShort(strVal);
				case DxfCode.ExtendedDataInteger32:
					return lineAsInt(strVal);
				case DxfCode.Invalid:
				default:
					throw new DxfException((int)code, this.Line);
			}
		}
	}
}
