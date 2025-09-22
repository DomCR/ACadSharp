using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class IOTests : IOTestsBase
	{
		public IOTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgEntitiesToDwgFile(FileModel test)
		{
			CadDocument doc = DwgReader.Read(test.Path);

			CadDocument transfer = new CadDocument();
			transfer.Header.Version = doc.Header.Version;

			List<Entity> entities = new List<Entity>(doc.Entities);
			foreach (var item in entities)
			{
				Entity e = doc.Entities.Remove(item);
				transfer.Entities.Add(e);
			}

			string file = Path.GetFileNameWithoutExtension(test.Path);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_moved_out.dwg");
			this.writeDwgFile(pathOut, transfer);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgEntitiesToDxfFile(FileModel test)
		{
			CadDocument doc = DwgReader.Read(test.Path);

			CadDocument transfer = new CadDocument();
			transfer.Header.Version = doc.Header.Version;

			List<Entity> entities = new List<Entity>(doc.Entities);
			foreach (var item in entities)
			{
				Entity e = doc.Entities.Remove(item);
				transfer.Entities.Add(e);
			}

			string file = Path.GetFileNameWithoutExtension(test.Path);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_moved_to.dxf");
			this.writeDxfFile(pathOut, transfer);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDwg(FileModel test)
		{
			CadDocument doc = DwgReader.Read(test.Path);

			string file = Path.GetFileNameWithoutExtension(test.Path);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_out.dwg");

			if (doc.Header.Version == ACadVersion.AC1032)
				return;

			this.writeDwgFile(pathOut, doc);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDxf(FileModel test)
		{
			CadDocument doc = DwgReader.Read(test.Path);

			string file = Path.GetFileNameWithoutExtension(test.Path);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_out.dxf");
			this.writeDxfFile(pathOut, doc);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void DxfEntitiesToDwgFile(FileModel test)
		{
			CadDocument doc = DxfReader.Read(test.Path);

			CadDocument transfer = new CadDocument();
			transfer.Header.Version = doc.Header.Version;

			List<Entity> entities = new List<Entity>(doc.Entities);
			foreach (var item in entities)
			{
				Entity e = doc.Entities.Remove(item);
				transfer.Entities.Add(e);
			}

			string file = Path.GetFileNameWithoutExtension(test.Path);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_moved_to.dwg");
			this.writeDwgFile(pathOut, transfer);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void DxfToDwg(FileModel test)
		{
			CadDocument doc = DxfReader.Read(test.Path);

			string file = Path.GetFileNameWithoutExtension(test.Path);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_dxf_to.dwg");

			this.writeDwgFile(pathOut, doc);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void DxfToDxf(FileModel test)
		{
			CadDocument doc = DxfReader.Read(test.Path);

			if (doc.Header.Version < ACadVersion.AC1012)
			{
				return;
			}

			string file = Path.GetFileNameWithoutExtension(test.Path);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_rewrite_out.dxf");
			this.writeDxfFile(pathOut, doc);
		}

		[Fact]
		public void EmptyDwgToDxf()
		{
			string inPath = Path.Combine($"{TestVariables.SamplesFolder}", "sample_base", "empty.dwg");
			CadDocument doc = DwgReader.Read(inPath);

			string file = Path.GetFileNameWithoutExtension(inPath);
			string pathOut = Path.Combine(TestVariables.OutputSamplesFolder, $"{file}_out.dxf");
			this.writeDxfFile(pathOut, doc);
		}

		protected virtual void writeDwgFile(string file, CadDocument doc)
		{
			if (!TestVariables.LocalEnv)
				return;

			if (!isSupportedVersion(doc.Header.Version))
				return;

			using (DwgWriter writer = new DwgWriter(file, doc))
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}
		}

		protected virtual void writeDxfFile(string file, CadDocument doc)
		{
			using (DxfWriter writer = new DxfWriter(file, doc, false))
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}
		}
	}
}