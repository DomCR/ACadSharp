using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public abstract class CadReaderTestsBase<T> : IDisposable
		where T : CadReaderBase
	{
		private const string _samplesFolder = "../../../../samples/";

		public static TheoryData<string> DwgFilePaths { get; }

		public static TheoryData<string> DxfAsciiFiles { get; }

		public static TheoryData<string> DxfBinaryFiles { get; }

		protected readonly Dictionary<string, CadDocument> _documents = new Dictionary<string, CadDocument>();	//TODO: this does not store the document readed

		protected readonly ITestOutputHelper _output;

		static CadReaderTestsBase()
		{
			DwgFilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, $"*.dwg"))
			{
				DwgFilePaths.Add(file);
			}

			DxfAsciiFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, "*_ascii.dxf"))
			{
				DxfAsciiFiles.Add(file);
			}

			DxfBinaryFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, "*_binary.dxf"))
			{
				DxfBinaryFiles.Add(file);
			}
		}

		public CadReaderTestsBase(ITestOutputHelper output)
		{
			this._output = output;
		}

		public virtual void ReadHeaderTest(string test)
		{
			using (T reader = (T)Activator.CreateInstance(typeof(T), test, null))
			{
				reader.OnNotificationHandler += this.onNotification;
				CadHeader header = reader.ReadHeader();
			}
		}

		public virtual void ReadTest(string test)
		{
			CadDocument doc = this.getDocument(test);

			Assert.NotNull(doc);
		}

		public virtual void AssertDocumentDefaults(string test)
		{
			CadDocument doc = this.getDocument(test);

			DocumentIntegrity.AssertDocumentDefaults(doc);
		}

		public virtual void AssertTableHirearchy(string test)
		{
			CadDocument doc = this.getDocument(test);

			DocumentIntegrity.AssertTableHirearchy(doc);
		}

		public virtual void AssertBlockRecords(string test)
		{
			CadDocument doc = this.getDocument(test);

			DocumentIntegrity.AssertBlockRecords(doc);
		}

		public void Dispose()
		{
			this._documents.Clear();
		}

		protected CadDocument getDocument(string path)
		{
			if (_documents.TryGetValue(path, out var doc))
				return doc;

			using (T reader = (T)Activator.CreateInstance(typeof(T), path, null))
			{
				reader.OnNotificationHandler += this.onNotification;
				doc = reader.Read();
			}

			_documents.Add(path, doc);

			return doc;
		}

		protected void onNotification(object sender, NotificationEventArgs e)
		{
			_output.WriteLine(e.Message);
		}
	}
}
