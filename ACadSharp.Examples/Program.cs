using ACadSharp.Entities;
using ACadSharp.Examples.Common;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Linq;

namespace ACadSharp.Examples
{
	class Program
	{
		const string _file = "../../../../samples/sample_AC1032.dwg";

		static void Main(string[] args)
		{
			CadDocument docc = new CadDocument();

			using (DwgReader reader = new DwgReader(_file))
			{
				docc = reader.Read();
			}

			CadDocument doc = new CadDocument();
			doc.Header.Version = ACadVersion.AC1018;

			BlockRecord record = new BlockRecord("my_block");
			record.Entities.Add(new Circle()
			{
				Center = new CSMath.XYZ(),
				Radius = 5
			});
			record.Entities.Add(new Line()
			{
				StartPoint = new CSMath.XYZ(),
				EndPoint = CSMath.XYZ.AxisX
			});

			doc.BlockRecords.Add((BlockRecord)docc.BlockRecords["MyBlock"].Clone());
			doc.BlockRecords.Add(record);

			Insert insert = new Insert(record);

			doc.Entities.Add(insert);
			doc.Entities.Add(new Insert(doc.BlockRecords["MyBlock"]));

			using (DxfWriter writer = new DxfWriter("D:\\Albert DC\\Desktop\\test\\insert_test.dxf", doc, false))
			{
				writer.Write();
			}

			using (DwgWriter writer = new DwgWriter("D:\\Albert DC\\Desktop\\test\\insert_test.dwg", doc))
			{
				writer.Write();
			}

			using (DxfReader reader = new DxfReader("D:\\Albert DC\\Desktop\\test\\insert_test.dxf"))
			{
				reader.Configuration = new DxfReaderConfiguration
				{
					Failsafe = true
				};
				reader.OnNotification += NotificationHelper.LogConsoleNotification;
				var test = reader.Read();
			}

			exploreDocument(doc);
		}

		/// <summary>
		/// Logs in the console the document information
		/// </summary>
		/// <param name="doc"></param>
		static void exploreDocument(CadDocument doc)
		{
			Console.WriteLine();
			Console.WriteLine("SUMMARY INFO:");
			Console.WriteLine($"\tTitle: {doc.SummaryInfo.Title}");
			Console.WriteLine($"\tSubject: {doc.SummaryInfo.Subject}");
			Console.WriteLine($"\tAuthor: {doc.SummaryInfo.Author}");
			Console.WriteLine($"\tKeywords: {doc.SummaryInfo.Keywords}");
			Console.WriteLine($"\tComments: {doc.SummaryInfo.Comments}");
			Console.WriteLine($"\tLastSavedBy: {doc.SummaryInfo.LastSavedBy}");
			Console.WriteLine($"\tRevisionNumber: {doc.SummaryInfo.RevisionNumber}");
			Console.WriteLine($"\tHyperlinkBase: {doc.SummaryInfo.HyperlinkBase}");
			Console.WriteLine($"\tCreatedDate: {doc.SummaryInfo.CreatedDate}");
			Console.WriteLine($"\tModifiedDate: {doc.SummaryInfo.ModifiedDate}");

			exploreTable(doc.AppIds);
			exploreTable(doc.BlockRecords);
			exploreTable(doc.DimensionStyles);
			exploreTable(doc.Layers);
			exploreTable(doc.LineTypes);
			exploreTable(doc.TextStyles);
			exploreTable(doc.UCSs);
			exploreTable(doc.Views);
			exploreTable(doc.VPorts);
		}

		static void exploreTable<T>(Table<T> table)
			where T : TableEntry
		{
			Console.WriteLine($"{table.ObjectName}");
			foreach (var item in table)
			{
				Console.WriteLine($"\tName: {item.Name}");

				if (item.Name == BlockRecord.ModelSpaceName && item is BlockRecord model)
				{
					Console.WriteLine($"\t\tEntities in the model:");
					foreach (var e in model.Entities.GroupBy(i => i.GetType().FullName))
					{
						Console.WriteLine($"\t\t{e.Key}: {e.Count()}");
					}
				}
			}
		}
	}
}
