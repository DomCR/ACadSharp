using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtilities.Text
{
	internal class TextEncoding : Encoding
	{
		/// <summary>
		/// <see cref="CodePage.Windows1252"/>
		/// </summary>
		/// <returns></returns>
		public static Encoding Windows1252()
		{
			return (Encoding)new TextEncoding(
				(int)CSUtilities.Text.CodePage.Windows1252,
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
			(byte) 0,
			(byte) 1,
			(byte) 2,
			(byte) 3,
			(byte) 4,
			(byte) 5,
			(byte) 6,
			(byte) 7,
			(byte) 8,
			(byte) 9,
			(byte) 10,
			(byte) 11,
			(byte) 12,
			(byte) 13,
			(byte) 14,
			(byte) 15,
			(byte) 16,
			(byte) 17,
			(byte) 18,
			(byte) 19,
			(byte) 20,
			(byte) 21,
			(byte) 22,
			(byte) 23,
			(byte) 24,
			(byte) 25,
			(byte) 26,
			(byte) 27,
			(byte) 28,
			(byte) 29,
			(byte) 30,
			(byte) 31,
			(byte) 32,
			(byte) 33,
			(byte) 34,
			(byte) 35,
			(byte) 36,
			(byte) 37,
			(byte) 38,
			(byte) 39,
			(byte) 40,
			(byte) 41,
			(byte) 42,
			(byte) 43,
			(byte) 44,
			(byte) 45,
			(byte) 46,
			(byte) 47,
			(byte) 48,
			(byte) 49,
			(byte) 50,
			(byte) 51,
			(byte) 52,
			(byte) 53,
			(byte) 54,
			(byte) 55,
			(byte) 56,
			(byte) 57,
			(byte) 58,
			(byte) 59,
			(byte) 60,
			(byte) 61,
			(byte) 62,
			(byte) 63,
			(byte) 64,
			(byte) 65,
			(byte) 66,
			(byte) 67,
			(byte) 68,
			(byte) 69,
			(byte) 70,
			(byte) 71,
			(byte) 72,
			(byte) 73,
			(byte) 74,
			(byte) 75,
			(byte) 76,
			(byte) 77,
			(byte) 78,
			(byte) 79,
			(byte) 80,
			(byte) 81,
			(byte) 82,
			(byte) 83,
			(byte) 84,
			(byte) 85,
			(byte) 86,
			(byte) 87,
			(byte) 88,
			(byte) 89,
			(byte) 90,
			(byte) 91,
			(byte) 92,
			(byte) 93,
			(byte) 94,
			(byte) 95,
			(byte) 96,
			(byte) 97,
			(byte) 98,
			(byte) 99,
			(byte) 100,
			(byte) 101,
			(byte) 102,
			(byte) 103,
			(byte) 104,
			(byte) 105,
			(byte) 106,
			(byte) 107,
			(byte) 108,
			(byte) 109,
			(byte) 110,
			(byte) 111,
			(byte) 112,
			(byte) 113,
			(byte) 114,
			(byte) 115,
			(byte) 116,
			(byte) 117,
			(byte) 118,
			(byte) 119,
			(byte) 120,
			(byte) 121,
			(byte) 122,
			(byte) 123,
			(byte) 124,
			(byte) 125,
			(byte) 126,
			(byte) 127,
			(byte) 63,
			(byte) 129,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 141,
			(byte) 63,
			(byte) 143,
			(byte) 144,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 63,
			(byte) 157,
			(byte) 63,
			(byte) 63,
			(byte) 160,
			(byte) 161,
			(byte) 162,
			(byte) 163,
			(byte) 164,
			(byte) 165,
			(byte) 166,
			(byte) 167,
			(byte) 168,
			(byte) 169,
			(byte) 170,
			(byte) 171,
			(byte) 172,
			(byte) 173,
			(byte) 174,
			(byte) 175,
			(byte) 176,
			(byte) 177,
			(byte) 178,
			(byte) 179,
			(byte) 180,
			(byte) 181,
			(byte) 182,
			(byte) 183,
			(byte) 184,
			(byte) 185,
			(byte) 186,
			(byte) 187,
			(byte) 188,
			(byte) 189,
			(byte) 190,
			(byte) 191,
			(byte) 192,
			(byte) 193,
			(byte) 194,
			(byte) 195,
			(byte) 196,
			(byte) 197,
			(byte) 198,
			(byte) 199,
			(byte) 200,
			(byte) 201,
			(byte) 202,
			(byte) 203,
			(byte) 204,
			(byte) 205,
			(byte) 206,
			(byte) 207,
			(byte) 208,
			(byte) 209,
			(byte) 210,
			(byte) 211,
			(byte) 212,
			(byte) 213,
			(byte) 214,
			(byte) 215,
			(byte) 216,
			(byte) 217,
			(byte) 218,
			(byte) 219,
			(byte) 220,
			(byte) 221,
			(byte) 222,
			(byte) 223,
			(byte) 224,
			(byte) 225,
			(byte) 226,
			(byte) 227,
			(byte) 228,
			(byte) 229,
			(byte) 230,
			(byte) 231,
			(byte) 232,
			(byte) 233,
			(byte) 234,
			(byte) 235,
			(byte) 236,
			(byte) 237,
			(byte) 238,
			(byte) 239,
			(byte) 240,
			(byte) 241,
			(byte) 242,
			(byte) 243,
			(byte) 244,
			(byte) 245,
			(byte) 246,
			(byte) 247,
			(byte) 248,
			(byte) 249,
			(byte) 250,
			(byte) 251,
			(byte) 252,
			(byte) 253,
			(byte) 254,
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
					return Encoding.Default;
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
					return TextEncoding.Windows1252();
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
					return Encoding.ASCII;
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
					return Encoding.UTF7;
				case Text.CodePage.Utf8:
					return Encoding.UTF8;
				default:
					return Encoding.Default;
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
			this.m_name = name;
			this.m_chars = chars;
			this.m_bytes = bytes;
			this.m_relation = new Dictionary<char, byte>();

			//Setup the relation
			for (int i = 0; i < byte.MaxValue; ++i)
			{
				char key = chars[i];
				if (key > 'ÿ')
					this.m_relation.Add(key, (byte)i);
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
				int index = (int)key;
				byte b;
				if (index < 256)
					b = this.m_bytes[index];
				else if (!this.m_relation.TryGetValue(key, out b))
					b = (byte)63;

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
				chars[charIndex] = this.m_chars[(int)bytes[byteIndex]];
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
