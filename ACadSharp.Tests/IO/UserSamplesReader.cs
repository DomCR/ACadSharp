using ACadSharp.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class UserSamplesReader : IOTestsBase
	{
		public static TheoryData<string> UserDwgFilePaths { get; }

		public static TheoryData<string> UserDxfFiles { get; }

		static UserSamplesReader()
		{
			string path = Path.Combine(_samplesFolder, "local", "user_files");
			UserDwgFilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(path, $"*.dwg"))
			{
				UserDwgFilePaths.Add(file);
			}

			UserDxfFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(path, $"*.dxf"))
			{
				UserDxfFiles.Add(file);
			}
		}

		public UserSamplesReader(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(UserDwgFilePaths))]
		public void ReadUserDwg(string test)
		{
			CadDocument doc = DwgReader.Read(test, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(UserDxfFiles))]
		public void ReadUserDxf(string test)
		{
			CadDocument doc = DxfReader.Read(test, this.onNotification);
		}
	}
}
