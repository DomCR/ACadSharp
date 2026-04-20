using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO;

public abstract class CadReaderTestsBase<T> : IOTestsBase, IDisposable
	where T : ICadReader
{
	protected readonly Dictionary<string, CadDocument> _documents = new Dictionary<string, CadDocument>();  //TODO: this does not store the document readed

	public CadReaderTestsBase(ITestOutputHelper output) : base(output)
	{
	}

	public virtual void AssertBlockRecords(FileModel test)
	{
		CadDocument doc = this.getDocument(test);

		this._docIntegrity.AssertBlockRecords(doc);
	}

	public virtual void AssertDocumentContent(FileModel test)
	{
		CadDocument doc = this.getDocument(test, false);

		if (doc.Header.Version < ACadVersion.AC1012)
		{
			//Older version do not keep the handles for tables and other objects like block_records
			return;
		}

		this._docIntegrity.AssertDocumentContent(doc);
	}

	public virtual void AssertDocumentDefaults(FileModel test)
	{
		CadDocument doc = this.getDocument(test);

		Assert.NotNull(doc.SummaryInfo);

		if (doc.Header.Version < ACadVersion.AC1012)
		{
			//Older version do not keep the handles for tables and other objects like block_records
			//This can be fixed if the document creates the default entries manually
			return;
		}

		this._docIntegrity.AssertDocumentDefaults(doc);
	}

	public virtual void AssertDocumentHeader(FileModel test)
	{
		CadDocument doc = this.getDocument(test, false);
		CadHeader header = doc.Header;

		Assert.Equal(doc.Layers[Layer.DefaultName], header.CurrentLayer);
		Assert.Equal(Layer.DefaultName, header.CurrentLayerName, ignoreCase: true);

		Assert.Equal(doc.LineTypes[LineType.ByLayerName], header.CurrentLineType);
		Assert.Equal(LineType.ByLayerName, header.CurrentLineTypeName, ignoreCase: true);

		Assert.Equal(doc.TextStyles[TextStyle.DefaultName], header.CurrentTextStyle);
		Assert.Equal(TextStyle.DefaultName, header.CurrentTextStyleName, ignoreCase: true);
	}

	public virtual void AssertDocumentTree(FileModel test)
	{
		CadDocument doc = this.getDocument(test, false);

		this._docIntegrity.AssertDocumentTree(doc);
	}

	public virtual void AssertTableHierarchy(FileModel test)
	{
		CadDocument doc = this.getDocument(test);

		this._docIntegrity.AssertTableHierarchy(doc);
	}

	public void Dispose()
	{
		this._documents.Clear();
	}

	public virtual void OnProgressTest(FileModel test)
	{
		CadDocument doc = this.getDocument(test, false, true);
	}

	public virtual void ReadHeaderTest(FileModel test)
	{
		using (T reader = (T)Activator.CreateInstance(typeof(T), test.Path, null))
		{
			reader.OnNotification += this.onNotification;
			CadHeader header = reader.ReadHeader();
		}
	}

	public virtual void ReadTest(FileModel test)
	{
		CadDocument doc = this.getDocument(test);

		Assert.NotNull(doc);
	}

	protected CadDocument getDocument(FileModel test, bool addNotificationEvent = true, bool addProgressEvent = false)
	{
		if (this._documents.TryGetValue(test.Path, out var doc))
			return doc;

		using (ICadReader reader = CadReader.CreateReader(test.Path))
		{
			if (addNotificationEvent)
			{
				reader.OnNotification += this.onNotification;
			}

			if (addProgressEvent)
			{
				reader.OnProgress += this.onProgress;
			}

			doc = reader.Read();
		}

		this._documents.Add(test.Path, doc);

		return doc;
	}
}