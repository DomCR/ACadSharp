using ACadSharp.Classes;
using ACadSharp.IO.DWG;
using Xunit;

namespace ACadSharp.Tests.IO
{
	public class AecProxyFallbackTests
	{
		[Theory]
		[InlineData("AEC_MODI", "TDbSymbModi", true)]
		[InlineData("AEC_SYMB_SECTION", "TDbSymbSection", true)]
		[InlineData("AEC_DBCONFIG", "TDbConfig", true)]
		[InlineData("AEC_DOOR", "AecDbDoor", true)]
		[InlineData("AECS_MEMBER", "AecsDbMember", true)]
		[InlineData("AECB_PIPE", "AecbDbPipe", true)]
		[InlineData("AEC_INTERFACESRECO", "AecInterfaceRecord", true)]
		[InlineData("SOME_CUSTOM_ENTITY", "AecDbCustom", true)]
		[InlineData("SOME_TIANZHENG_ENTITY", "TDbCustom", true)]
		[InlineData("aec_wall", "AecDbWall", true)]
		[InlineData("ACDBDICTIONARYWDFLT", "AcDbDictionaryWithDefault", false)]
		[InlineData("ACDBDETAILVIEWSTYLE", "AcDbDetailViewStyle", false)]
		[InlineData("CELLSTYLEMAP", "AcDbCellStyleMap", false)]
		[InlineData("LINE", "AcDbLine", false)]
		public void IsAecClass_DetectsAecFamilies(string dxfName, string cppClassName, bool expected)
		{
			DxfClass dxfClass = new DxfClass
			{
				DxfName = dxfName,
				CppClassName = cppClassName,
			};

			Assert.Equal(expected, DwgObjectReader.IsAecClass(dxfClass));
		}

		[Fact]
		public void IsAecClass_DetectsAecApplicationName()
		{
			DxfClass dxfClass = new DxfClass
			{
				DxfName = "SOME_CUSTOM_ENTITY",
				CppClassName = "SomeCustomObject",
				ApplicationName = "AEC_KERNAL|missing interpreter",
			};

			Assert.True(DwgObjectReader.IsAecClass(dxfClass));
		}

		[Fact]
		public void IsAecClass_NullClass_ReturnsFalse()
		{
			Assert.False(DwgObjectReader.IsAecClass(null));
		}
	}
}
