using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Utils.Text
{
	internal class TextEncoding : Encoding
	{
		/// <summary>
		/// <see cref="CodePage.Windows1252"/>
		/// </summary>
		/// <returns></returns>
		public static Encoding Windows1252()
		{
			return new TextEncoding(
				(int)Text.CodePage.Windows1252,
				"Windows1252",
						new char[256]
					{
			char.MinValue,
			'\x0001',
			'\x0002',
			'\x0003',
			'\x0004',
			'\x0005',
			'\x0006',
			'\a',
			'\b',
			'\t',
			'\n',
			'\v',
			'\f',
			'\r',
			'\x000E',
			'\x000F',
			'\x0010',
			'\x0011',
			'\x0012',
			'\x0013',
			'\x0014',
			'\x0015',
			'\x0016',
			'\x0017',
			'\x0018',
			'\x0019',
			'\x001A',
			'\x001B',
			'\x001C',
			'\x001D',
			'\x001E',
			'\x001F',
			' ',
			'!',
			'"',
			'#',
			'$',
			'%',
			'&',
			'\'',
			'(',
			')',
			'*',
			'+',
			',',
			'-',
			'.',
			'/',
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			':',
			';',
			'<',
			'=',
			'>',
			'?',
			'@',
			'A',
			'B',
			'C',
			'D',
			'E',
			'F',
			'G',
			'H',
			'I',
			'J',
			'K',
			'L',
			'M',
			'N',
			'O',
			'P',
			'Q',
			'R',
			'S',
			'T',
			'U',
			'V',
			'W',
			'X',
			'Y',
			'Z',
			'[',
			'\\',
			']',
			'^',
			'_',
			'`',
			'a',
			'b',
			'c',
			'd',
			'e',
			'f',
			'g',
			'h',
			'i',
			'j',
			'k',
			'l',
			'm',
			'n',
			'o',
			'p',
			'q',
			'r',
			's',
			't',
			'u',
			'v',
			'w',
			'x',
			'y',
			'z',
			'{',
			'|',
			'}',
			'~',
			'\x007F',
			'€',
			'\x0081',
			'‚',
			'ƒ',
			'„',
			'…',
			'†',
			'‡',
			'ˆ',
			'‰',
			'Š',
			'‹',
			'Œ',
			'\x008D',
			'Ž',
			'\x008F',
			'\x0090',
			'‘',
			'’',
			'“',
			'”',
			'•',
			'–',
			'—',
			'˜',
			'™',
			'š',
			'›',
			'œ',
			'\x009D',
			'ž',
			'Ÿ',
			' ',
			'¡',
			'¢',
			'£',
			'¤',
			'¥',
			'¦',
			'§',
			'¨',
			'©',
			'ª',
			'«',
			'¬',
			'\x00AD',
			'®',
			'¯',
			'°',
			'±',
			'\x00B2',
			'\x00B3',
			'´',
			'µ',
			'¶',
			'·',
			'¸',
			'\x00B9',
			'º',
			'»',
			'\x00BC',
			'\x00BD',
			'\x00BE',
			'¿',
			'À',
			'Á',
			'Â',
			'Ã',
			'Ä',
			'Å',
			'Æ',
			'Ç',
			'È',
			'É',
			'Ê',
			'Ë',
			'Ì',
			'Í',
			'Î',
			'Ï',
			'Ð',
			'Ñ',
			'Ò',
			'Ó',
			'Ô',
			'Õ',
			'Ö',
			'×',
			'Ø',
			'Ù',
			'Ú',
			'Û',
			'Ü',
			'Ý',
			'Þ',
			'ß',
			'à',
			'á',
			'â',
			'ã',
			'ä',
			'å',
			'æ',
			'ç',
			'è',
			'é',
			'ê',
			'ë',
			'ì',
			'í',
			'î',
			'ï',
			'ð',
			'ñ',
			'ò',
			'ó',
			'ô',
			'õ',
			'ö',
			'÷',
			'ø',
			'ù',
			'ú',
			'û',
			'ü',
			'ý',
			'þ',
			'ÿ'
					},
						new byte[256]
					{
			 0,
			 1,
			 2,
			 3,
			 4,
			 5,
			 6,
			 7,
			 8,
			 9,
			 10,
			 11,
			 12,
			 13,
			 14,
			 15,
			 16,
			 17,
			 18,
			 19,
			 20,
			 21,
			 22,
			 23,
			 24,
			 25,
			 26,
			 27,
			 28,
			 29,
			 30,
			 31,
			 32,
			 33,
			 34,
			 35,
			 36,
			 37,
			 38,
			 39,
			 40,
			 41,
			 42,
			 43,
			 44,
			 45,
			 46,
			 47,
			 48,
			 49,
			 50,
			 51,
			 52,
			 53,
			 54,
			 55,
			 56,
			 57,
			 58,
			 59,
			 60,
			 61,
			 62,
			 63,
			 64,
			 65,
			 66,
			 67,
			 68,
			 69,
			 70,
			 71,
			 72,
			 73,
			 74,
			 75,
			 76,
			 77,
			 78,
			 79,
			 80,
			 81,
			 82,
			 83,
			 84,
			 85,
			 86,
			 87,
			 88,
			 89,
			 90,
			 91,
			 92,
			 93,
			 94,
			 95,
			 96,
			 97,
			 98,
			 99,
			 100,
			 101,
			 102,
			 103,
			 104,
			 105,
			 106,
			 107,
			 108,
			 109,
			 110,
			 111,
			 112,
			 113,
			 114,
			 115,
			 116,
			 117,
			 118,
			 119,
			 120,
			 121,
			 122,
			 123,
			 124,
			 125,
			 126,
			 127,
			 63,
			 129,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 141,
			 63,
			 143,
			 144,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 63,
			 157,
			 63,
			 63,
			 160,
			 161,
			 162,
			 163,
			 164,
			 165,
			 166,
			 167,
			 168,
			 169,
			 170,
			 171,
			 172,
			 173,
			 174,
			 175,
			 176,
			 177,
			 178,
			 179,
			 180,
			 181,
			 182,
			 183,
			 184,
			 185,
			 186,
			 187,
			 188,
			 189,
			 190,
			 191,
			 192,
			 193,
			 194,
			 195,
			 196,
			 197,
			 198,
			 199,
			 200,
			 201,
			 202,
			 203,
			 204,
			 205,
			 206,
			 207,
			 208,
			 209,
			 210,
			 211,
			 212,
			 213,
			 214,
			 215,
			 216,
			 217,
			 218,
			 219,
			 220,
			 221,
			 222,
			 223,
			 224,
			 225,
			 226,
			 227,
			 228,
			 229,
			 230,
			 231,
			 232,
			 233,
			 234,
			 235,
			 236,
			 237,
			 238,
			 239,
			 240,
			 241,
			 242,
			 243,
			 244,
			 245,
			 246,
			 247,
			 248,
			 249,
			 250,
			 251,
			 252,
			 253,
			 254,
			byte.MaxValue
					});
		}
		/// <summary>
		/// Gets the encoding for specified <see cref="CodePage"/>
		/// </summary>
		public static Encoding GetListedEncoding(CodePage code)
		{
			switch (code)
			{
				case Text.CodePage.Unknown:
					return Default;
				case Text.CodePage.Ibm037:
					break;
				case Text.CodePage.Ibm437:
					break;
				case Text.CodePage.Asmo708:
					break;
				case Text.CodePage.Dos720:
					break;
				case Text.CodePage.Ibm737:
					break;
				case Text.CodePage.Ibm775:
					break;
				case Text.CodePage.Ibm850:
					break;
				case Text.CodePage.Ibm852:
					break;
				case Text.CodePage.Ibm855:
					break;
				case Text.CodePage.Ibm857:
					break;
				case Text.CodePage.Ibm860:
					break;
				case Text.CodePage.Ibm861:
					break;
				case Text.CodePage.Dos862:
					break;
				case Text.CodePage.Ibm863:
					break;
				case Text.CodePage.Ibm864:
					break;
				case Text.CodePage.Ibm865:
					break;
				case Text.CodePage.Cp866:
					break;
				case Text.CodePage.Ibm869:
					break;
				case Text.CodePage.Ibm870:
					break;
				case Text.CodePage.Windows874:
					break;
				case Text.CodePage.Cp875:
					break;
				case Text.CodePage.Shift_jis:
					break;
				case Text.CodePage.Gb2312:
					break;
				case Text.CodePage.Ksc5601:
					break;
				case Text.CodePage.big5:
					break;
				case Text.CodePage.Ibm1026:
					break;
				case Text.CodePage.Ibm01047:
					break;
				case Text.CodePage.Ibm01140:
					break;
				case Text.CodePage.Ibm01141:
					break;
				case Text.CodePage.Ibm01142:
					break;
				case Text.CodePage.Ibm01143:
					break;
				case Text.CodePage.Ibm01144:
					break;
				case Text.CodePage.Ibm01145:
					break;
				case Text.CodePage.Ibm01146:
					break;
				case Text.CodePage.Ibm01147:
					break;
				case Text.CodePage.Ibm01148:
					break;
				case Text.CodePage.Ibm01149:
					break;
				case Text.CodePage.Utf16:
					break;
				case Text.CodePage.UnicodeFFFE:
					break;
				case Text.CodePage.Windows1250:
					break;
				case Text.CodePage.Windows1251:
					break;
				case Text.CodePage.Windows1252:
					return Windows1252();
				case Text.CodePage.Windows1253:
					break;
				case Text.CodePage.Windows1254:
					break;
				case Text.CodePage.Windows1255:
					break;
				case Text.CodePage.Windows1256:
					break;
				case Text.CodePage.Windows1257:
					break;
				case Text.CodePage.Windows1258:
					break;
				case Text.CodePage.Johab:
					break;
				case Text.CodePage.Macintosh:
					break;
				case Text.CodePage.Xmacjapanese:
					break;
				case Text.CodePage.Xmacchinesetrad:
					break;
				case Text.CodePage.Xmackorean:
					break;
				case Text.CodePage.Xmacarabic:
					break;
				case Text.CodePage.Xmachebrew:
					break;
				case Text.CodePage.Xmacgreek:
					break;
				case Text.CodePage.Xmaccyrillic:
					break;
				case Text.CodePage.Xmacchinesesimp:
					break;
				case Text.CodePage.Xmacromanian:
					break;
				case Text.CodePage.Xmacukrainian:
					break;
				case Text.CodePage.Xmacthai:
					break;
				case Text.CodePage.Xmacce:
					break;
				case Text.CodePage.Xmacicelandic:
					break;
				case Text.CodePage.Xmacturkish:
					break;
				case Text.CodePage.Xmaccroatian:
					break;
				case Text.CodePage.Utf32:
					break;
				case Text.CodePage.Utf32BE:
					break;
				case Text.CodePage.XChineseCNS:
					break;
				case Text.CodePage.Xcp20001:
					break;
				case Text.CodePage.XChineseEten:
					break;
				case Text.CodePage.Xcp20003:
					break;
				case Text.CodePage.Xcp20004:
					break;
				case Text.CodePage.Xcp20005:
					break;
				case Text.CodePage.XIA5:
					break;
				case Text.CodePage.XIA5German:
					break;
				case Text.CodePage.XIA5Swedish:
					break;
				case Text.CodePage.XIA5Norwegian:
					break;
				case Text.CodePage.Usascii:
					return ASCII;
				case Text.CodePage.Xcp20261:
					break;
				case Text.CodePage.Xcp20269:
					break;
				case Text.CodePage.Ibm273:
					break;
				case Text.CodePage.Ibm277:
					break;
				case Text.CodePage.Ibm278:
					break;
				case Text.CodePage.Ibm280:
					break;
				case Text.CodePage.Ibm284:
					break;
				case Text.CodePage.Ibm285:
					break;
				case Text.CodePage.Ibm290:
					break;
				case Text.CodePage.Ibm297:
					break;
				case Text.CodePage.Ibm420:
					break;
				case Text.CodePage.Ibm423:
					break;
				case Text.CodePage.Ibm424:
					break;
				case Text.CodePage.XEBCDICKoreanExtended:
					break;
				case Text.CodePage.IbmThai:
					break;
				case Text.CodePage.Koi8r:
					break;
				case Text.CodePage.Ibm871:
					break;
				case Text.CodePage.Ibm880:
					break;
				case Text.CodePage.Ibm905:
					break;
				case Text.CodePage.Ibm00924:
					break;
				case Text.CodePage.EUCJP:
					break;
				case Text.CodePage.Xcp20936:
					break;
				case Text.CodePage.Xcp20949:
					break;
				case Text.CodePage.Cp1025:
					break;
				case Text.CodePage.Koi8u:
					break;
				case Text.CodePage.Iso88591:
					break;
				case Text.CodePage.Iso88592:
					break;
				case Text.CodePage.Iso88593:
					break;
				case Text.CodePage.Iso88594:
					break;
				case Text.CodePage.Iso88595:
					break;
				case Text.CodePage.Iso88596:
					break;
				case Text.CodePage.Iso88597:
					break;
				case Text.CodePage.Iso88598:
					break;
				case Text.CodePage.Iso88599:
					break;
				case Text.CodePage.Iso885913:
					break;
				case Text.CodePage.Iso885915:
					break;
				case Text.CodePage.XEuropa:
					break;
				case Text.CodePage.Iso88598i:
					break;
				case Text.CodePage.Iso2022jp:
					break;
				case Text.CodePage.CsISO2022JP:
					break;
				case Text.CodePage.Iso2022jp_jis:
					break;
				case Text.CodePage.Iso2022kr:
					break;
				case Text.CodePage.Xcp50227:
					break;
				case Text.CodePage.Eucjp:
					break;
				case Text.CodePage.EUCCN:
					break;
				case Text.CodePage.Euckr:
					break;
				case Text.CodePage.Hzgb2312:
					break;
				case Text.CodePage.Gb18030:
					break;
				case Text.CodePage.Xisciide:
					break;
				case Text.CodePage.Xisciibe:
					break;
				case Text.CodePage.Xisciita:
					break;
				case Text.CodePage.Xisciite:
					break;
				case Text.CodePage.Xisciias:
					break;
				case Text.CodePage.Xisciior:
					break;
				case Text.CodePage.Xisciika:
					break;
				case Text.CodePage.Xisciima:
					break;
				case Text.CodePage.Xisciigu:
					break;
				case Text.CodePage.Xisciipa:
					break;
				case Text.CodePage.Utf7:
					return UTF7;
				case Text.CodePage.Utf8:
					return UTF8;
				default:
					return Default;
			}

			return null;
		}
		//**************************************************************************
		private string m_name;
		private char[] m_chars;
		private byte[] m_bytes;
		private Dictionary<char, byte> m_relation;
		public TextEncoding(int code, string name, char[] chars, byte[] bytes) : base(code)
		{
			m_name = name;
			m_chars = chars;
			m_bytes = bytes;
			m_relation = new Dictionary<char, byte>();

			//Setup the relation
			for (int i = 0; i < byte.MaxValue; ++i)
			{
				char key = chars[i];
				if (key > 'ÿ')
					m_relation.Add(key, (byte)i);
			}
		}
		//**************************************************************************
		/// <inheritdoc/>
		public override int GetByteCount(char[] chars, int index, int count)
		{
			return count;
		}
		/// <inheritdoc/>
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			int total = charIndex + charCount;
			while (charIndex < total)
			{

				char key = chars[charIndex];
				int index = key;
				byte b;
				if (index < 256)
					b = m_bytes[index];
				else if (!m_relation.TryGetValue(key, out b))
					b = 63;

				bytes[byteIndex] = b;
				++charIndex;
				++byteIndex;
			}
			return charCount;
		}
		/// <inheritdoc/>
		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return count;
		}
		/// <inheritdoc/>
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			for (int index = byteIndex + byteCount; byteIndex < index; ++byteIndex)
			{
				chars[charIndex] = m_chars[bytes[byteIndex]];
				++charIndex;
			}
			return byteCount;
		}
		/// <inheritdoc/>
		public override int GetMaxByteCount(int charCount)
		{
			return charCount;
		}
		/// <inheritdoc/>
		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount;
		}
	}
}
