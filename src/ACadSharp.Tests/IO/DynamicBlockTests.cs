using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class DynamicBlockTests : IOTestsBase
	{
		public static TheoryData<FileModel> GenericDynamicBlocksPaths { get; } = new();

		public static TheoryData<FileModel> IsolatedDynamicBlocksPaths { get; } = new();

		static DynamicBlockTests()
		{
			loadSamples("./", "dxf", GenericDynamicBlocksPaths);
			loadSamples("./", "dwg", GenericDynamicBlocksPaths);

			loadSamples("./dynamic-blocks", "*dwg", IsolatedDynamicBlocksPaths);
			loadSamples("./dynamic-blocks", "*dxf", IsolatedDynamicBlocksPaths);
		}

		public DynamicBlockTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(GenericDynamicBlocksPaths))]
		public void DynamicBlocksTest(FileModel test)
		{
			CadDocument doc;

			if (test.IsDxf)
			{
				DxfReaderConfiguration configuration = new();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DxfReader.Read(test.Path, configuration, this.onNotification);

				if (doc.Header.Version <= ACadVersion.AC1021)
				{
					return;
				}
			}
			else
			{
				DwgReaderConfiguration configuration = new DwgReaderConfiguration();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DwgReader.Read(test.Path, configuration, this.onNotification);
			}

			string dynamicName = "my-dynamic-block";

			BlockRecord blk = doc.BlockRecords[dynamicName];

			Assert.True(blk.IsDynamic);

			//Dictionary entry
			EvaluationGraph eval = blk.XDictionary.GetEntry<EvaluationGraph>("ACAD_ENHANCEDBLOCK");

			//Extended data related to the dynamic block
			var a = blk.ExtendedData.Get(doc.AppIds["AcDbBlockRepETag"]);
			var b = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockTrueName"]);
			var c = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockGUID"]);

			Insert basic = doc.GetCadObject<Insert>(0xABA);
			Insert modified = doc.GetCadObject<Insert>(0xAC5);

			Assert.NotNull(modified.Block.Source);
			Assert.Equal(dynamicName, modified.Block.Source.Name);
		}

		[Theory]
		[MemberData(nameof(IsolatedDynamicBlocksPaths))]
		public void IsolatedTest(FileModel test)
		{
			var config = getConfiguration(test);
			var doc = this.readDocument(test, config);

			switch (test.NoExtensionName)
			{
				case DxfFileToken.ObjectBlockVisibilityParameter:
					this.assertVisibilityParameter(doc);
					break;
				case DxfFileToken.ObjectBlockRotationParameter:
					this.assertRotationParameter(doc);
					break;
				case DxfFileToken.ObjectBlockPointParameter:
					this.assertPointParameter(doc);
					break;
				case DxfFileToken.ObjectBlockLinearParameter:
					this.assertLinearParameter(doc);
					break;
				default:
					throw new System.NotImplementedException();
			}
		}

		[Theory]
		[MemberData(nameof(IsolatedDynamicBlocksPaths))]
		public void DwgWriterNoDanglingDictionaryEntriesTest(FileModel test)
		{
			CadDocument doc = this.readDocument(test, this.getConfiguration(test));

			if (!this.isSupportedVersion(doc.Header.Version))
			{
				return;
			}

			this.rewriteDwgInMemory(doc, out List<NotificationEventArgs> notifications);

			this.assertNoDanglingDictionaryEntries(notifications);
		}

		[Theory]
		[MemberData(nameof(IsolatedDynamicBlocksPaths))]
		public void DxfWriterNoDanglingDictionaryEntriesTest(FileModel test)
		{
			CadDocument doc = this.readDocument(test, this.getConfiguration(test));

			this.rewriteDxfInMemory(doc, out List<NotificationEventArgs> notifications);

			this.assertNoDanglingDictionaryEntries(notifications);
		}

		private void assertNoDanglingDictionaryEntries(List<NotificationEventArgs> notifications)
		{
			//A dictionary entry pointing to an object that has not been written
			//triggers an "Entry not found" warning when the output is read back
			Assert.DoesNotContain(notifications, e => e.Message != null && e.Message.Contains("Entry not found"));
		}

		private CadDocument rewriteDwgInMemory(CadDocument doc, out List<NotificationEventArgs> reReadNotifications)
		{
			List<NotificationEventArgs> notifications = new();

			MemoryStream stream = new MemoryStream();
			DwgWriter.Write(stream, doc, new DwgWriterConfiguration { WriteXRecords = true }, this.onNotification);

			reReadNotifications = notifications;
			return DwgReader.Read(new MemoryStream(stream.ToArray()), (s, e) =>
			{
				notifications.Add(e);
				this.onNotification(s, e);
			});
		}

		private CadDocument rewriteDxfInMemory(CadDocument doc, out List<NotificationEventArgs> reReadNotifications)
		{
			List<NotificationEventArgs> notifications = new();

			MemoryStream stream = new MemoryStream();
			DxfWriter.Write(stream, doc, false, new DxfWriterConfiguration { WriteXRecords = true }, this.onNotification);

			reReadNotifications = notifications;
			return DxfReader.Read(new MemoryStream(stream.ToArray()), (s, e) =>
			{
				notifications.Add(e);
				this.onNotification(s, e);
			});
		}

		private void assertLinearParameter(CadDocument doc)
		{
			var original = doc.BlockRecords["LINEAR_PARAM"];
			foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
			{
				Assert.Equal(original, record.Source);
			}

			foreach (Insert insert in doc.Entities.OfType<Insert>())
			{
				if (insert.XDictionary == null)
				{
					continue;
				}

				var dict = insert.XDictionary.GetEntry<CadDictionary>("AcDbBlockRepresentation");
				var representation = dict.GetEntry<BlockRepresentationData>("AcDbRepData");

				Assert.NotEmpty(insert.Block.Source.EvaluationGraph.Nodes.Select(n => n.Expression).OfType<BlockLinearParameter>());

				Assert.NotNull(representation);
				Assert.Equal(original, representation.Block);

				XRecord record = insert.XDictionary
					.GetEntry<CadDictionary>("AcDbBlockRepresentation")
					.GetEntry<CadDictionary>("AppDataCache")
					.GetEntry<CadDictionary>("ACAD_ENHANCEDBLOCKDATA")
					.OfType<XRecord>().First();
			}
		}

		private void assertPointParameter(CadDocument doc)
		{
			//Not implemented in this PR
		}

		private void assertRotationParameter(CadDocument doc)
		{
			var original = doc.BlockRecords["dynamic_block"];
			foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
			{
				Assert.Equal(original, record.Source);
			}

			foreach (Insert insert in doc.Entities.OfType<Insert>())
			{
				if (insert.XDictionary == null)
				{
					continue;
				}

				var dict = insert.XDictionary.GetEntry<CadDictionary>("AcDbBlockRepresentation");
				var representation = dict.GetEntry<BlockRepresentationData>("AcDbRepData");

				Assert.NotEmpty(insert.Block.Source.EvaluationGraph.Nodes.Select(n => n.Expression).OfType<BlockRotationParameter>());

				Assert.NotNull(representation);
				Assert.Equal(original, representation.Block);

				XRecord record = insert.XDictionary
					.GetEntry<CadDictionary>("AcDbBlockRepresentation")
					.GetEntry<CadDictionary>("AppDataCache")
					.GetEntry<CadDictionary>("ACAD_ENHANCEDBLOCKDATA")
					.OfType<XRecord>().First();
			}
		}

		private void assertVisibilityParameter(CadDocument doc)
		{
			var original = doc.BlockRecords["block_visibility_parameter"];
			foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
			{
				Assert.Equal(original, record.Source);
			}

			foreach (Insert insert in doc.Entities.OfType<Insert>())
			{
				var dict = insert.XDictionary.GetEntry<CadDictionary>("AcDbBlockRepresentation");
				var representation = dict.GetEntry<BlockRepresentationData>("AcDbRepData");

				Assert.NotEmpty(insert.Block.Source.EvaluationGraph.Nodes.Select(n => n.Expression).OfType<BlockVisibilityParameter>());

				Assert.NotNull(representation);
				Assert.Equal(original, representation.Block);

				XRecord record = insert.XDictionary
					.GetEntry<CadDictionary>("AcDbBlockRepresentation")
					.GetEntry<CadDictionary>("AppDataCache")
					.GetEntry<CadDictionary>("ACAD_ENHANCEDBLOCKDATA")
					.OfType<XRecord>().First();

				var name = record.Entries.FirstOrDefault(e => e.Code == 1).Value as string;
				Assert.False(string.IsNullOrEmpty(name));
			}
		}
	}
}