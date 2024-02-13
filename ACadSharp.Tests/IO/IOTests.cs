using ACadSharp.Entities;
using ACadSharp.IO;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class IOTests : IOTestsBase
	{
		public IOTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void EmptyDwgToDxf()
		{
			string inPath = Path.Combine($"{samplesFolder}", "sample_base", "empty.dwg");
			CadDocument doc = DwgReader.Read(inPath);

			string file = Path.GetFileNameWithoutExtension(inPath);
			string pathOut = Path.Combine(samplesOutFolder, $"{file}_out.dxf");
			this.writeDxfFile(pathOut, doc);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDwg(string test)
		{
			CadDocument doc = DwgReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(samplesOutFolder, $"{file}_out.dwg");

			this.writeDwgFile(pathOut, doc);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDxf(string test)
		{
			CadDocument doc = DwgReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(samplesOutFolder, $"{file}_out.dxf");
			this.writeDxfFile(pathOut, doc);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void DxfToDxf(string test)
		{
			CadDocument doc = DxfReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(samplesOutFolder, $"{file}_rewrite_out.dxf");
			this.writeDxfFile(pathOut, doc);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgEntitiesToDwgFile(string test)
		{
			CadDocument doc = DwgReader.Read(test);

			CadDocument transfer = new CadDocument();
			transfer.Header.Version = doc.Header.Version;

			List<Entity> entities = new List<Entity>(doc.Entities);
			foreach (var item in entities)
			{
				Entity e = doc.Entities.Remove(item);
				transfer.Entities.Add(e);
			}

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(samplesOutFolder, $"{file}_moved_out.dwg");
			this.writeDwgFile(pathOut, transfer);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgEntitiesToDxfFile(string test)
		{
			CadDocument doc = DwgReader.Read(test);

			CadDocument transfer = new CadDocument();
			transfer.Header.Version = doc.Header.Version;

			List<Entity> entities = new List<Entity>(doc.Entities);
			foreach (var item in entities)
			{
				Entity e = doc.Entities.Remove(item);
				transfer.Entities.Add(e);
			}

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(samplesOutFolder, $"{file}_moved_out.dxf");
			this.writeDxfFile(pathOut, transfer);
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
