using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfReaderTests : CadReaderTestsBase<DxfReader>
	{
		public DxfReaderTests(ITestOutputHelper output) : base(output) { }

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void ReadHeaderAciiTest(string test)
		{
			base.ReadHeaderTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadHeaderBinaryTest(string test)
		{
			base.ReadHeaderTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void ReadAsciiTest(string test)
		{
			base.ReadTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadBinaryTest(string test)
		{
			base.ReadTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertDocumentDefaults(string test)
		{
			base.AssertDocumentDefaults(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertTableHirearchy(string test)
		{
			base.AssertTableHirearchy(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertBlockRecords(string test)
		{
			base.AssertBlockRecords(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertDocumentTree(string test)
		{
			base.AssertDocumentTree(test);
		}
	}
}