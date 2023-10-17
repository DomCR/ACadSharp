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
			string inPath = Path.Combine($"{_samplesFolder}", "sample_base", "empty.dwg");
			CadDocument doc = DwgReader.Read(inPath);

			string file = Path.GetFileNameWithoutExtension(inPath);
			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_out.dxf");
			this.writeDxfFile(pathOut, doc, true);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDwg(string test)
		{
			CadDocument doc = DwgReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_out.dwg");

			//accoreconsole always fails because cannot recover the file
			this.writeDwgFile(pathOut, doc, false);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDxf(string test)
		{
			CadDocument doc = DwgReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_out.dxf");
			this.writeDxfFile(pathOut, doc, true);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void DxfToDxf(string test)
		{
			CadDocument doc = DxfReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);
			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_rewrite_out.dxf");
			this.writeDxfFile(pathOut, doc, true);
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
			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_moved_out.dwg");
			this.writeDwgFile(pathOut, transfer, false);
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
			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_moved_out.dxf");
			this.writeDxfFile(pathOut, transfer, true);
		}

		private void writeCadFile(string file, CadWriterBase writer, bool check)
		{
			using (writer)
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}

			if (check)
				this.checkDxfDocumentInAutocad(Path.GetFullPath(file));
		}

		private void writeDwgFile(string file, CadDocument doc, bool check)
		{
			if (doc.Header.Version < ACadVersion.AC1014 || doc.Header.Version > ACadVersion.AC1018)
				return;

			using (DwgWriter writer = new DwgWriter(file, doc))
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}

			if (true)
				this.checkDwgDocumentInAutocad(Path.GetFullPath(file));
		}

		private void writeDxfFile(string file, CadDocument doc, bool check)
		{
			using (DxfWriter writer = new DxfWriter(file, doc, false))
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}

			if (check)
				this.checkDxfDocumentInAutocad(Path.GetFullPath(file));
		}

		private void writeDwgFile(string file, CadDocument doc)
		{
			if (doc.Header.Version > ACadVersion.AC1018)
				return;

			using (DwgWriter writer = new DwgWriter(file, doc))
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}
		}
	}
}
