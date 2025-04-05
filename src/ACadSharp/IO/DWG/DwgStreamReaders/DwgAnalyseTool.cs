using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG.DwgStreamReaders {
	internal static class DwgAnalyseTool {

		public static void ShowCurrentPosAndShift(IDwgStreamReader objectReader, IDwgStreamReader handlesReader) {
			long positionO = objectReader.Position;
			int bitShiftO = objectReader.BitShift;
			System.Diagnostics.Debug.WriteLine($"Pos: {positionO}, bShi: {bitShiftO}");
			long positionH = handlesReader.Position;
			int bitShiftH = handlesReader.BitShift;
			System.Diagnostics.Debug.WriteLine($"Pos: {positionH}, bShi: {bitShiftH}");
		}


		public static void Analyse02(IDwgStreamReader objectReader, IDwgStreamReader handlesReader, int count = 20) {
			System.Diagnostics.Debug.WriteLine("");
			System.Diagnostics.Debug.WriteLine("...................");

			long positionInitial = objectReader.Position;
			int bitShiftInitial = objectReader.BitShift;
			int testCount = count;
			for (int a = 0; a < testCount; a++) {
				long positionBefore = objectReader.Position;
				int bitShiftBefore = objectReader.BitShift;

				var b = objectReader.ReadBit();
				resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);

				var b2 = objectReader.Read2Bits();

				if (b2 < 3) {
					try {
						resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
						var by = objectReader.ReadByte();
						resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
						var s = objectReader.ReadBitShort();
						resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
						var l = objectReader.ReadBitLong();
						resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
						var d = objectReader.ReadBitDouble();
						System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore},  b: {b}, \tb2: {b2}, \tbyte: {by}, \ts: {s}, \tl: {l}, \td: {d}");
					}
					catch (Exception ex) {
						System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore},  b: {b}, \tb2: {b2}, Exception: {ex.Message}");
					}
					resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
				}
				else {
					System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore},  b: {b}, b2: {b2} -- no value");
					resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
				}

				//	Advance 1 bit
				var dummy = objectReader.ReadBit();
			}
			resetPosition(objectReader, handlesReader, positionInitial, bitShiftInitial);
		}


		public static void Analyse03(IDwgStreamReader objectReader, IDwgStreamReader handlesReader, IDwgStreamReader textReader, string fieldType, object value, int count = 20) {
			long positionInitial = objectReader.Position;
			int bitShiftInitial = objectReader.BitShift;
			int testCount = count;
			for (int a = 0; a < testCount; a++) {
				//System.Diagnostics.Debug.WriteLine(".");
				long positionBefore = objectReader.Position;
				int bitShiftBefore = objectReader.BitShift;

				var b2 = objectReader.Read2Bits();
				resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);

				if (b2 < 3) {
					try {
						switch (fieldType) {
						case "BS":
							var s = objectReader.ReadBitShort();
							if (value == null || s == Convert.ToInt16(value)) {
								System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore}, b2: {b2}, s: {s}");
							}
							break;
						case "BL":
							var l = objectReader.ReadBitLong();
							if (value == null || l == Convert.ToInt32(value)) {
								System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore}, b2: {b2}, l: {l}");
							}
							break;
						case "BD":
							var d = objectReader.ReadBitDouble();
							if (value == null || d >= Convert.ToDouble(value) - 0.1 && d < Convert.ToDouble(value) + 0.1) {
								System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore}, b2: {b2}, d: {d}");
							}
							break;
						case "VT":
							var t = textReader.ReadVariableText();
							break;
						}
					}
					catch (Exception ex) {
						//System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore}, b2: {b2}, d: ?");
					}
					resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
				}
				else {
					if (value == null) {
						System.Diagnostics.Debug.WriteLine($"Pos: {positionBefore}, bShi: {bitShiftBefore}, b2: {b2} -- no value");
					}
					resetPosition(objectReader, handlesReader, positionBefore, bitShiftBefore);
				}

				var dummy = objectReader.ReadBit();
			}
			resetPosition(objectReader, handlesReader, positionInitial, bitShiftInitial);
		}


		public static void resetPosition(IDwgStreamReader objectReader, IDwgStreamReader handlesReader, long positionBefore, int bitShiftBefore) {
			objectReader.Position = positionBefore - 1;
			objectReader.ReadByte();
			((DwgStreamReaderBase)objectReader).BitShift = bitShiftBefore;
		}
	}
}
