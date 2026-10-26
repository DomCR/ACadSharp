using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using CSMath;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class FieldStringRoundTripTests
{
	[Fact]
	public void FieldWithoutFormatStringIsWritten()
	{
		//writeField measured FormatString.Length without checking for null: a field without format string (the
		//object's default) threw a NullReferenceException.
		Field field = this.readBack(this.textWithField("abc", formatString: null));
		Assert.Equal("abc", field.Value.Value);
		Assert.Equal("after the value", field.Value.FormattedValue);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void EmptyStringValueDoesNotShiftTheObject(string value)
	{
		//writeStringCadValue wrote a count of 0 for an empty string and then went on to write the string bytes
		//as well: null threw, and "" shifted the rest of the object, which the reader then dropped.
		Field field = this.readBack(this.textWithField(value, formatString: string.Empty));
		Assert.True(string.IsNullOrEmpty(field.Value.Value as string));
		Assert.Equal(CadValueUnitType.Distance, field.Value.Units);
		Assert.Equal("%lu2", field.Value.Format);
		Assert.Equal("after the value", field.Value.FormattedValue);
	}

	private CadDocument textWithField(string value, string formatString)
	{
		CadDocument doc = new CadDocument();
		MText text = new MText { Value = "x", InsertPoint = XYZ.Zero, Height = 1 };
		doc.Entities.Add(text);

		Field field = new Field { EvaluatorId = "_text", FieldCode = "%<\\_FldIdx 0>%", FormatString = formatString };
		field.Value.ValueType = CadValueType.String;
		if (value != null)
		{
			field.Value.SetValue(value, CadValueType.String);
		}
		field.Value.IsEmpty = false;
		field.Value.Units = CadValueUnitType.Distance;
		field.Value.Format = "%lu2";
		field.Value.FormattedValue = "after the value";

		CadDictionary fields = new CadDictionary("ACAD_FIELD");
		text.CreateExtendedDictionary().Add(fields);
		fields.Add("TEXT", field);
		return doc;
	}

	private Field readBack(CadDocument doc)
	{
		string path = Path.Combine(Path.GetTempPath(), $"acadsharp-{Guid.NewGuid():N}.dwg");
		try
		{
			using (FileStream fs = File.Create(path))
			{
				new DwgWriter(fs, doc).Write();
			}

			CadDocument read = DwgReader.Read(path);
			MText text = read.Entities.OfType<MText>().Single();
			Assert.True(text.XDictionary.TryGetEntry<CadDictionary>("ACAD_FIELD", out CadDictionary fields));
			Assert.True(fields.TryGetEntry<Field>("TEXT", out Field field));
			return field;
		}
		finally
		{
			File.Delete(path);
		}
	}
}
