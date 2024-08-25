﻿using ACadSharp.IO;
using ACadSharp.Tests.Common;
using ACadSharp.Tests.TestModels;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public abstract class IOTestsBase
	{
		public static TheoryData<FileModel> DwgFilePaths { get; } = new();

		public static TheoryData<FileModel> DxfAsciiFiles { get; } = new();

		public static TheoryData<FileModel> DxfBinaryFiles { get; } = new();

		public static TheoryData<ACadVersion> Versions { get; }

		protected readonly DwgReaderConfiguration _dwgConfiguration = new DwgReaderConfiguration
		{
			Failsafe = false
		};

		protected readonly DxfReaderConfiguration _dxfConfiguration = new DxfReaderConfiguration
		{
		};

		protected readonly ITestOutputHelper _output;

		protected readonly DocumentIntegrity _docIntegrity;

		static IOTestsBase()
		{
			loadSamples("", "dwg", DwgFilePaths);
			loadSamples("", "dxf", DxfAsciiFiles);
			loadSamples("", "dxf", DxfBinaryFiles);

			Versions = new TheoryData<ACadVersion>
			{
				ACadVersion.AC1012,
				ACadVersion.AC1014,
				ACadVersion.AC1015,
				ACadVersion.AC1018,
				ACadVersion.AC1021,
				ACadVersion.AC1024,
				ACadVersion.AC1027,
				ACadVersion.AC1032
			};
		}

		public IOTestsBase(ITestOutputHelper output)
		{
			this._output = output;
			this._docIntegrity = new DocumentIntegrity(output);
		}

		protected void onNotification(object sender, NotificationEventArgs e)
		{
			if (e.NotificationType == NotificationType.Error)
			{
				throw e.Exception;
			}

			_output.WriteLine(e.Message);
			if (e.Exception != null)
			{
				_output.WriteLine(e.Exception.ToString());
			}
		}

		protected static void loadLocalSamples(string folder, string ext, TheoryData<FileModel> files)
		{
			string path = Path.Combine("local", folder);
			loadSamples(path, ext, files);
		}

		protected static void loadSamples(string folder, string ext, TheoryData<FileModel> files)
		{
			string path = TestVariables.SamplesFolder;

			if (!string.IsNullOrEmpty(folder))
			{
				path = Path.Combine(TestVariables.SamplesFolder, folder);
			}

			if (!Directory.Exists(path))
			{
				files.Add(new FileModel());
				return;
			}

			foreach (string file in Directory.GetFiles(path, $"*.{ext}"))
			{
				files.Add(new FileModel(file));
			}

			if (!files.Any())
			{
				files.Add(new FileModel());
			}
		}

		protected bool isSupportedVersion(ACadVersion version)
		{
			switch (version)
			{
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
				case ACadVersion.AC1012:
					return false;
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
				case ACadVersion.AC1018:
					return true;
				case ACadVersion.AC1021:
					return false;
				case ACadVersion.AC1024:
					return true;
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					return true;
				case ACadVersion.Unknown:
				default:
					return false;
			}
		}
	}
}
