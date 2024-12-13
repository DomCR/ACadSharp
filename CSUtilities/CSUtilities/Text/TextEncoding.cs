using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtilities.Text
{
	internal partial class TextEncoding : Encoding
	{
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

		public override string BodyName => this._name;

		public override string EncodingName => this._name;

		public override string HeaderName => this._name;

		public override string WebName => this._name;

		private string _name;
		
		private char[] _chars;

		private byte[] _bytes;

		private Dictionary<char, byte> m_relation;

		public TextEncoding(int code, string name, char[] chars, byte[] bytes) : base(code)
		{
			this._name = name;
			this._chars = chars;
			this._bytes = bytes;
			this.m_relation = new Dictionary<char, byte>();

			//Setup the relation
			for (int i = 0; i < byte.MaxValue; ++i)
			{
				char key = chars[i];
				if (key > 'ÿ')
					this.m_relation.Add(key, (byte)i);
			}
		}

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
					b = this._bytes[index];
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
				chars[charIndex] = this._chars[(int)bytes[byteIndex]];
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
