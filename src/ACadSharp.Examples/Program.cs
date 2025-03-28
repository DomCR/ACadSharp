using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using Svg;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ACadSharp.Examples
{
	class Program
	{
		const string _file = "../../../../../samples/sample_AC1032.dwg";
		const string _previewFile = "../../../../../samples/svg/export_sample.dwg";

		static void Main(string[] args)
		{
			CadDocument doc;
			DwgPreview preview;
			using (DwgReader reader = new DwgReader(_previewFile))
			{
				doc = reader.Read();
				preview = reader.ReadPreview();
			}

			//exploreDocument(doc);

			var svgStream = new MemoryStream();
			using (SvgWriter svgwriter = new SvgWriter(svgStream, doc))
			{
				svgwriter.Write();
			}

			var pngStream = new MemoryStream();
			SvgDocument svg = SvgDocument.Open<SvgDocument>(new MemoryStream(svgStream.GetBuffer()));
			var bitmap = svg.Draw();
			bitmap.Save(pngStream, ImageFormat.Png);

			string dwgOutput = Path.Combine(Path.GetDirectoryName(_previewFile),
				"out",
				$"{Path.GetFileNameWithoutExtension(_file)}.out.dwg");
			using (DwgWriter writer = new DwgWriter(dwgOutput, new CadDocument()))
			{
				writer.Preview = preview;
				writer.Write();
			}
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