using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Utils.Text
{
	/// <summary>
	/// Code page <seealso cref="Encoding"/>.
	/// </summary>
	/// <remarks>
	/// Source: https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding?view=net-5.0
	/// </remarks>
	public enum CodePage
	{
		/// <summary>
		/// Unknown code page
		/// </summary>
		Unknown = 0x00000000,
		/// <summary>
		/// IBM EBCDIC (US-Canada)
		/// </summary>
		Ibm037 = 37,
		/// <summary>
		/// OEM United States
		/// </summary>
		Ibm437 = 437,
		/// <summary>
		/// Arabic (ASMO 708)
		/// </summary>
		Asmo708 = 708, // 0x000002D0
		/// <summary>
		/// Arabic (DOS)
		/// </summary>
		Dos720 = 720, // 0x000002D0
		/// <summary>
		/// Greek (DOS)
		/// </summary>
		Ibm737 = 737, // 0x000002E1
		/// <summary>
		/// Baltic (DOS)
		/// </summary>
		Ibm775 = 775, // 0x00000307
		/// <summary>
		/// Western European (DOS)
		/// </summary>
		Ibm850 = 850, // 0x00000352
		/// <summary>
		/// Central European (DOS)
		/// </summary>
		Ibm852 = 852, // 0x00000354
		/// <summary>
		/// 	OEM Cyrillic
		/// </summary>
		Ibm855 = 855, // 0x00000357
		/// <summary>
		/// Turkish (DOS)
		/// </summary>
		Ibm857 = 857, // 0x00000359
		/// <summary>
		/// Portuguese (DOS)
		/// </summary>
		Ibm860 = 860, // 0x0000035C
		/// <summary>
		/// Icelandic (DOS)
		/// </summary>
		Ibm861 = 861, // 0x0000035D
		/// <summary>
		/// Hebrew (DOS)
		/// </summary>
		Dos862 = 862, // 0x0000035E
		/// <summary>
		/// French Canadian (DOS)
		/// </summary>
		Ibm863 = 863, // 0x0000035F
		/// <summary>
		/// Arabic (864)
		/// </summary>
		Ibm864 = 864, // 0x00000360
		/// <summary>
		/// Nordic (DOS)
		/// </summary>
		Ibm865 = 865, // 0x00000361
		/// <summary>
		/// Cyrillic (DOS)
		/// </summary>
		Cp866 = 866, // 0x00000362
		/// <summary>
		/// Greek, Modern (DOS)
		/// </summary>
		Ibm869 = 869, // 0x00000365
		/// <summary>
		/// IBM EBCDIC (Multilingual Latin-2)
		/// </summary>
		Ibm870 = 870,
		/// <summary>
		/// Thai (Windows)
		/// </summary>
		Windows874 = 874, // 0x0000036A
		/// <summary>
		/// IBM EBCDIC (Greek Modern)
		/// </summary>
		Cp875 = 875,
		/// <summary>
		/// Japanese (Shift-JIS)
		/// </summary>
		Shift_jis = 932, // 0x000003A4
		/// <summary>
		/// Chinese simplified code page (GB2312)
		/// </summary>
		Gb2312 = 936, // 0x000003A8
		/// <summary>
		/// Korean
		/// </summary>
		Ksc5601 = 949, // 0x000003B5
		/// <summary>
		/// Chinese Traditional (Big5)
		/// </summary>
		big5 = 950, // 0x000003B6
		Ibm1026 = 1026,//IBM EBCDIC (Turkish Latin-5)
		Ibm01047 = 1047,//IBM Latin-1
		Ibm01140 = 1140,//IBM EBCDIC (US-Canada-Euro)
		Ibm01141 = 1141,//IBM EBCDIC (Germany-Euro)
		Ibm01142 = 1142,//IBM EBCDIC (Denmark-Norway-Euro)
		Ibm01143 = 1143,//IBM EBCDIC (Finland-Sweden-Euro)
		Ibm01144 = 1144,//IBM EBCDIC (Italy-Euro)
		Ibm01145 = 1145,//IBM EBCDIC (Spain-Euro)
		Ibm01146 = 1146,//IBM EBCDIC (UK-Euro)
		Ibm01147 = 1147,//IBM EBCDIC (France-Euro)
		Ibm01148 = 1148,//IBM EBCDIC (International-Euro)
		Ibm01149 = 1149,//IBM EBCDIC (Icelandic-Euro)
		Utf16 = 1200,//Unicode
		UnicodeFFFE = 1201,//Unicode (Big endian)
		Windows1250 = 1250,//Central European (Windows)
		Windows1251 = 1251,//Cyrillic (Windows)
		Windows1252 = 1252,//Western European (Windows)
		Windows1253 = 1253,//Greek (Windows)
		Windows1254 = 1254,//Turkish (Windows)
		Windows1255 = 1255,//Hebrew (Windows)
		Windows1256 = 1256,//Arabic (Windows)
		Windows1257 = 1257,//Baltic (Windows)
		Windows1258 = 1258,//Vietnamese (Windows)
		Johab = 1361,//Korean (Johab)
		Macintosh = 10000, //Western European (Mac)
		Xmacjapanese = 10001, //Japanese (Mac)
		Xmacchinesetrad = 10002, //Chinese Traditional (Mac)
		Xmackorean = 10003, //Korean (Mac)
		Xmacarabic = 10004, //Arabic (Mac)
		Xmachebrew = 10005, //Hebrew (Mac)
		Xmacgreek = 10006, //Greek (Mac)
		Xmaccyrillic = 10007, //Cyrillic (Mac)
		Xmacchinesesimp = 10008, //Chinese Simplified (Mac)
		Xmacromanian = 10010, //Romanian (Mac)
		Xmacukrainian = 10017, //Ukrainian (Mac)
		Xmacthai = 10021, //Thai (Mac)
		Xmacce = 10029, //Central European (Mac)
		Xmacicelandic = 10079, //Icelandic (Mac)
		Xmacturkish = 10081, //Turkish (Mac)
		Xmaccroatian = 10082, //Croatian (Mac)
		Utf32 = 12000, //Unicode (UTF-32)
		Utf32BE = 12001, //Unicode (UTF-32 Big endian)
		XChineseCNS = 20000, //Chinese Traditional (CNS)
		Xcp20001 = 20001, //TCA Taiwan
		XChineseEten = 20002, //Chinese Traditional (Eten)
		Xcp20003 = 20003, //IBM5550 Taiwan
		Xcp20004 = 20004, //TeleText Taiwan
		Xcp20005 = 20005, //Wang Taiwan
		XIA5 = 20105, //Western European (IA5)
		XIA5German = 20106, //German (IA5)
		XIA5Swedish = 20107, //Swedish (IA5)
		XIA5Norwegian = 20108, //Norwegian (IA5)
		Usascii = 20127, //US-ASCII
		Xcp20261 = 20261, //T.61
		Xcp20269 = 20269, //ISO-6937
		Ibm273 = 20273, //IBM EBCDIC (Germany)
		Ibm277 = 20277, //IBM EBCDIC (Denmark-Norway)
		Ibm278 = 20278, //IBM EBCDIC (Finland-Sweden)
		Ibm280 = 20280, //IBM EBCDIC (Italy)
		Ibm284 = 20284, //IBM EBCDIC (Spain)
		Ibm285 = 20285, //IBM EBCDIC (UK)
		Ibm290 = 20290, //IBM EBCDIC (Japanese katakana)
		Ibm297 = 20297, //IBM EBCDIC (France)
		Ibm420 = 20420, //IBM EBCDIC (Arabic)
		Ibm423 = 20423, //IBM EBCDIC (Greek)
		Ibm424 = 20424, //IBM EBCDIC (Hebrew)
		XEBCDICKoreanExtended = 20833, //IBM EBCDIC (Korean Extended)
		IbmThai = 20838, //IBM EBCDIC (Thai)
		Koi8r = 20866, //Cyrillic (KOI8-R)
		Ibm871 = 20871, //IBM EBCDIC (Icelandic)
		Ibm880 = 20880, //IBM EBCDIC (Cyrillic Russian)
		Ibm905 = 20905, //IBM EBCDIC (Turkish)
		Ibm00924 = 20924, //IBM Latin-1
		EUCJP = 20932, //Japanese (JIS 0208-1990 and 0212-1990)
		Xcp20936 = 20936, //Chinese Simplified (GB2312-80)
		Xcp20949 = 20949, //Korean Wansung
		Cp1025 = 21025, //IBM EBCDIC (Cyrillic Serbian-Bulgarian)
		Koi8u = 21866, //Cyrillic (KOI8-U)
		Iso88591 = 28591, //Western European (ISO)
		Iso88592 = 28592, //Central European (ISO)
		Iso88593 = 28593, //Latin 3 (ISO)
		Iso88594 = 28594, //Baltic (ISO)
		Iso88595 = 28595, //Cyrillic (ISO)
		Iso88596 = 28596, //Arabic (ISO)
		Iso88597 = 28597, //Greek (ISO)
		Iso88598 = 28598, //Hebrew (ISO-Visual)
		Iso88599 = 28599, //Turkish (ISO)
		Iso885913 = 28603, //Estonian (ISO)
		Iso885915 = 28605, //Latin 9 (ISO)
		XEuropa = 29001, //Europa
		Iso88598i = 38598, //Hebrew (ISO-Logical)
		Iso2022jp = 50220, //Japanese (JIS)
		CsISO2022JP = 50221, //Japanese (JIS-Allow 1 byte Kana)
		Iso2022jp_jis = 50222, //Japanese (JIS-Allow 1 byte Kana - SO/SI)
		Iso2022kr = 50225, //Korean (ISO)
		Xcp50227 = 50227, //Chinese Simplified (ISO-2022)
		Eucjp = 51932, //Japanese (EUC)
		EUCCN = 51936, //Chinese Simplified (EUC)
		Euckr = 51949, //Korean (EUC)
		Hzgb2312 = 52936, //Chinese Simplified (HZ)
		Gb18030 = 54936, //Chinese Simplified (GB18030)
		Xisciide = 57002, //ISCII Devanagari
		Xisciibe = 57003, //ISCII Bengali
		Xisciita = 57004, //ISCII Tamil
		Xisciite = 57005, //ISCII Telugu
		Xisciias = 57006, //ISCII Assamese
		Xisciior = 57007, //ISCII Oriya
		Xisciika = 57008, //ISCII Kannada
		Xisciima = 57009, //ISCII Malayalam
		Xisciigu = 57010, //ISCII Gujarati
		Xisciipa = 57011, //ISCII Punjabi
		Utf7 = 65000, //Unicode (UTF-7)
		Utf8 = 65001, //Unicode (UTF-8)
	}
}