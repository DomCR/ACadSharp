﻿using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using Xunit;
using Xunit.Abstractions;

//https://stackoverflow.com/questions/29105838/find-the-center-of-a-circle-x-and-y-position-with-only-2-random-points-and-bul
//http://iainmnorman.github.io/curvebulgecalc/
//http://www.lee-mac.com/bulgeconversion.html

namespace ACadSharp.Tests.IO.Issues
{
	public class Issue_62 : IOTestsBase
	{
		public static TheoryData<string> IssueDwgFile { get; } = new TheoryData<string>();

		public static TheoryData<string> IssueDxfFiles { get; } = new TheoryData<string>();

		public Issue_62(ITestOutputHelper output) : base(output)
		{
			loadSamples("issue_files", "dwg", IssueDwgFile);
			loadSamples("issue_files", "dxf", IssueDxfFiles);
		}

		[Theory]
		[MemberData(nameof(IssueDwgFile))]
		public void Issue64Test(string test)
		{
			if (string.IsNullOrEmpty(test) ||
				Path.GetFileNameWithoutExtension(test) != "issue_62")
				return;

			CadDocument doc = DwgReader.Read(test, this.onNotification);

			foreach (LwPolyline pl in doc.Entities)
			{
				pl.Explode();
			}
		
		}
	}
}