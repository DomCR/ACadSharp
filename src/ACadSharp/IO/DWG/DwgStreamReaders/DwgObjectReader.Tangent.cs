using ACadSharp.Classes;
using ACadSharp.Entities.Tangent;
using ACadSharp.IO.DWG.Tangent;
using ACadSharp.IO.Templates;
using CSMath;
using System;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectReader
{
	private const string _tchDataMapBegin = "TDBXDATAMAP_BLOCK_BEGIN_";

	private const string _tchDataMapEnd = "TDBXDATAMAP_BLOCK_END_";

	private double readTangentDouble()
	{
		string cipher = this._textReader.ReadVariableText();

		if (!TgDataEncrypt.IsCiphertext(cipher))
		{
			this._builder.Notify($"Tianzheng value is not ciphertext: {cipher}", NotificationType.Warning);
			return double.NaN;
		}

		if (TgDataEncrypt.TryDecode(cipher, out double value, TgDataEncrypt.ObjectXor))
		{
			return value;
		}

		this._builder.Notify($"Tianzheng ciphertext could not be decoded: {cipher}", NotificationType.Warning);
		return double.NaN;
	}

	private int readTangentInt()
	{
		double value = this.readTangentDouble();
		return double.IsNaN(value) ? 0 : (int)value;
	}

	private CadTemplate readTangentObject(DxfClass dxfClass)
	{
		if (dxfClass == null)
		{
			return null;
		}

		switch (dxfClass.DxfName)
		{
			case TchDxfClasses.Text:
				return this.readTangentText(dxfClass);
			default:
				return null;
		}
	}

	private byte[] readTangentObjectHeader()
	{
		long dataStart = this._objectReader.PositionInBits();

		//Insertion point, after a prefix of 82 bits
		//All three are identical:
		//0A 11 90 55 05 90 39 03 81 04 + 2 bits: 01
		//00001010 00010001 10010000 01010101 00000101 10010000 00111001 00000011 10000001 00000100 01
		var arr = this._objectReader.ReadBytes(10);
		short last = this._objectReader.Read2Bits();

		return arr;
	}

	/// <summary>
	/// Reads a Tianzheng text (TCH_TEXT).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Object stream: an 82 bit prefix that is not decoded, followed by three bit doubles,
	/// the second always 0.0, that hold the insertion point as Y, 0, X.
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
	/// value
	/// height
	/// rotation?        (#0.0)
	/// int              (#7, color or alignment?)
	/// int              (#0)
	/// </code>
	/// </para>
	/// </remarks>
	private CadTemplate readTangentText(DxfClass dxfClass)
	{
		TchText entity = new TchText(dxfClass);
		CadEntityTemplate<TchText> template = new(entity);

		this.readCommonEntityData(template);

		try
		{
			this.readTangentObjectHeader();

			double y = this._objectReader.ReadBitDouble();
			//Not sure
			double z = this._objectReader.ReadBitDouble();
			double x = this._objectReader.ReadBitDouble();
			entity.InsertionPoint = new XYZ(x, y, 0);

			entity.ClassId = this._textReader.ReadVariableText();
			entity.Scale = this.readTangentDouble();

			this.readTangentDouble();
			this.readTangentInt();

			string marker = this._textReader.ReadVariableText();
			if (marker != _tchDataMapBegin)
			{
				this._builder.Notify($"Tianzheng object {entity.ObjectName} {entity.Handle} has no {_tchDataMapBegin}, found: {marker}", NotificationType.Warning);
				return template;
			}

			string entry = this._textReader.ReadVariableText();
			while (entry != _tchDataMapEnd)
			{
				if (entry == _tchDataMapEnd)
				{
					break;
				}
				entity.DataMap.Add(entry);

				entry = this._textReader.ReadVariableText();
			}

			entity.StyleId = this._textReader.ReadVariableText();
			this.readTangentInt();

			//Content
			entity.Value = this._textReader.ReadVariableText();
			entity.Height = this.readTangentDouble();
			//Rotation?
			var rotation = this.readTangentDouble();
			//Color or alignment, 7 in every sample
			var unknown1 = this.readTangentInt();
			var unknown2 = this.readTangentInt();
		}
		catch (Exception ex)
		{
			this._builder.Notify($"Tianzheng object {dxfClass.DxfName} {entity.Handle} could not be read", NotificationType.Warning, ex);
		}

		return template;
	}
}