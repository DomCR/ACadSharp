using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class LeaderAnnotationRoundTripTests
{
	[Theory]
	[InlineData("dwg")]
	[InlineData("dxf")]
	public void AssociatedAnnotationIsWritten(string format)
	{
		//The DWG writer wrote the 340 handle as null (AutoCAD's AUDIT then reports "Bad mtext id") and the DXF
		//writer had the line commented out; the readers always read it.
		CadDocument doc = new CadDocument();
		MText note = new MText { Value = "NOTE", InsertPoint = new XYZ(20, 10, 0), Height = 2.5 };
		Leader leader = new Leader { CreationType = LeaderCreationType.CreatedWithTextAnnotation };
		leader.Vertices.Add(new XYZ(0, 0, 0));
		leader.Vertices.Add(new XYZ(18, 10, 0));
		doc.Entities.Add(note);
		doc.Entities.Add(leader);
		leader.AssociatedAnnotation = note;

		CadDocument read = this.writeAndRead(doc, format);

		MText readNote = Assert.IsType<MText>(read.Entities.OfType<Leader>().Single().AssociatedAnnotation);
		Assert.Equal("NOTE", readNote.Value);
	}

	private CadDocument writeAndRead(CadDocument doc, string format)
	{
		string path = Path.Combine(Path.GetTempPath(), $"acadsharp-{Guid.NewGuid():N}.{format}");
		try
		{
			if (format == "dwg")
			{
				using (FileStream fs = File.Create(path))
				{
					new DwgWriter(fs, doc).Write();
				}

				return DwgReader.Read(path);
			}

			new DxfWriter(path, doc, false).Write();
			return DxfReader.Read(path);
		}
		finally
		{
			File.Delete(path);
		}
	}
}
