using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Entities.Tangent;
using ACadSharp.IO.DWG.Tangent;
using ACadSharp.IO.Templates;
using CSMath;
using CSUtilities.IO;
using System;
using System.Text;

namespace ACadSharp.IO.DWG;

/// <summary>
/// Reading the Tianzheng (TArch / Tangent) custom objects.
/// </summary>
/// <remarks>
/// Kept apart from the rest of <see cref="DwgObjectReader"/> because these objects are not
/// AutoCAD classes: they have no entry in the file's class table that names a reader, so they
/// arrive as unlisted objects and are recognised by their class's DXF name instead.
/// </remarks>
internal partial class DwgObjectReader
{
	private const string _tchDataMapBegin = "TDBXDATAMAP_BLOCK_BEGIN_";

	private const string _tchDataMapEnd = "TDBXDATAMAP_BLOCK_END_";

	private static bool isDrawingCoordinate(double value)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return false;
		}

		double abs = Math.Abs(value);
		return abs == 0 || (abs > 1e-6 && abs < 1e9);
	}

	/// <summary>
	/// Finds the insertion point in the object stream.
	/// </summary>
	/// <remarks>
	/// The prefix before it differs per class (84 bits for text, 44 for a drawing name), so it is
	/// located by shape: a full bit double (code 00), a zero bit double (code 10) and another full
	/// bit double, both values finite and in drawing range.
	/// </remarks>
	private bool findTangentPoint(long dataStart, out long pointStart)
	{
		for (long offset = 0; offset < 256; offset++)
		{
			long p = dataStart + offset;

			this._objectReader.SetPositionInBits(p);
			if (this._objectReader.Read2Bits() != 0)
			{
				continue;
			}

			double first = this._objectReader.ReadDouble();
			if (this._objectReader.Read2Bits() != 2 || this._objectReader.Read2Bits() != 0)
			{
				continue;
			}

			double second = this._objectReader.ReadDouble();
			if (isDrawingCoordinate(first) && isDrawingCoordinate(second))
			{
				pointStart = p;
				return true;
			}
		}

		pointStart = -1;
		return false;
	}

	/// <summary>
	/// Reads the data every Tianzheng entity starts with.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Object stream: a prefix that differs per class and is not decoded, followed by three
	/// bit doubles, the second always 0.0, that hold the insertion point.
	/// </para>
	/// <para>
	/// String stream, numbers written as <see cref="TgDataEncrypt"/> ciphertext:
	/// <code>
	/// class guid
	/// scale            (#100 for 1:100)
	/// double           (#0.0)
	/// int              (#0)
	/// TDBXDATAMAP_BLOCK_BEGIN_
	///   entries        (T20d1, T20d2, ...)
	/// TDBXDATAMAP_BLOCK_END_
	/// style guid
	/// int              (#0)
	/// class specific fields
	/// </code>
	/// </para>
	/// </remarks>
	/// <returns>False when the data does not have the expected shape.</returns>
	private bool readTangentCommonData(TchEntity entity)
	{
		long dataStart = this._objectReader.PositionInBits();

		entity.ClassId = this._textReader.ReadVariableText();

		if (this.findTangentPoint(dataStart, out long pointStart))
		{
			this._objectReader.SetPositionInBits(pointStart);

			double y = this._objectReader.ReadBitDouble();
			this._objectReader.ReadBitDouble();
			double x = this._objectReader.ReadBitDouble();

			if (entity is TchText text)
			{
				text.InsertionPoint = new XYZ(x, y, 0);
			}
		}
		else
		{
			this._builder.Notify($"Insertion point not found for Tianzheng object {entity.ObjectName} {entity.Handle}", NotificationType.Warning);
		}

		entity.Scale = this.readTangentDouble();

		this.readTangentDouble();
		this.readTangentInt();

		string marker = this._textReader.ReadVariableText();
		if (marker != _tchDataMapBegin)
		{
			this._builder.Notify($"Tianzheng object {entity.ObjectName} {entity.Handle} has no {_tchDataMapBegin}, found: {marker}", NotificationType.Warning);
			return false;
		}

		//TDBXDATAMAP_BLOCK_BEGIN_ found: the entries run up to the end marker.
		for (int i = 0; i < 256; i++)
		{
			string entry = this._textReader.ReadVariableText();
			if (entry == _tchDataMapEnd)
			{
				//TDBXDATAMAP_BLOCK_END_ found: the block is closed.
				break;
			}

			entity.DataMap.Add(entry);
		}

		entity.StyleId = this._textReader.ReadVariableText();
		this.readTangentInt();

		return true;
	}

	private double readTangentDouble()
	{
		string cipher = this._textReader.ReadVariableText();

		if (!TgDataEncrypt.IsCiphertext(cipher))
		{
			return double.NaN;
		}

		if (TgDataEncrypt.TryDecode(cipher, out double value, TgDataEncrypt.ObjectXor))
		{
			return value;
		}

		return double.NaN;
	}

	private int readTangentInt()
	{
		double value = this.readTangentDouble();
		return double.IsNaN(value) ? 0 : (int)value;
	}

	/// <summary>
	/// Reads a Tianzheng class whose object is text.
	/// </summary>
	/// <remarks>
	/// The object's own data is kept while it is being read, because the fields in it are not at
	/// offsets the filer can reach: the string stream that holds them is located from the end of
	/// the object rather than walked to, and the position is found by trying both of the
	/// alignments these classes use. Nothing of that reaches the entity, which is handed the
	/// fields rather than the bytes.
	/// </remarks>
	private CadTemplate readTangentText(DxfClass dxfClass)
	{
		TchText entity = new TchText(dxfClass);
		CadEntityTemplate<TchText> template = new(entity);

		this.readCommonEntityData(template);

		try
		{
			if (!this.readTangentCommonData(entity))
			{
				return template;
			}
		}
		catch (Exception ex)
		{
			this._builder.Notify($"Tianzheng object {dxfClass.DxfName} {entity.Handle} could not be read", NotificationType.Warning, ex);
		}

		//Content
		entity.Value = this._textReader.ReadVariableText();
		entity.Height = this.readTangentDouble();
		//Rotation?
		var rot = this.readTangentDouble();
		//Color or alignment, 7 in every sample
		var v1 = this.readTangentInt();
		var v2 = this.readTangentInt();
		
		return template;
	}

	private void readTangentTextData(DxfClass dxfClass, TchText entity)
	{
		if (!this.readTangentCommonData(entity))
		{
			return;
		}

		//Class specific fields, they follow the content in the string stream.
		switch (dxfClass.DxfName)
		{
			case TchDxfClasses.MText:
				//Content, the lines are separated by \n
				entity.Value = this._textReader.ReadVariableText();
				entity.Height = this.readTangentDouble();
				//Rotation?
				this.readTangentDouble();
				//Alignment?, 10 in the sample
				this.readTangentInt();
				this.readTangentInt();
				entity.Width = this.readTangentDouble();
				entity.LineSpacingFactor = this.readTangentDouble();
				break;
			case TchDxfClasses.DrawingName:
				entity.Value = this._textReader.ReadVariableText();
				entity.Subtitle = this._textReader.ReadVariableText();
				//Rotation?
				this.readTangentDouble();
				entity.Height = this.readTangentDouble();
				entity.SubtitleHeight = this.readTangentDouble();
				//Width factors of title and subtitle?, 0.7 and 0.6 in the sample
				this.readTangentDouble();
				this.readTangentDouble();
				this.readTangentInt();
				this.readTangentInt();
				this.readTangentInt();
				break;
		}
	}
}