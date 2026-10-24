using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Entities.Tangent;
using ACadSharp.IO.DWG.Tangent;
using ACadSharp.IO.Templates;
using CSMath;
using CSUtilities.IO;
using System;

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

		//var b = this.readProxyEntity();

		this.readCommonEntityData(template);

		//The class specific fields have no reader in the filer, so the bytes are taken and what
		//can be read from them is read here rather than by the entity.
		long payloadStart = this._objectReader.PositionInBits();

		//Guid
		var guid = this._textReader.ReadVariableText();
		
		byte[] payload = this.readRemainingObjectData();
		if (payload == null || payload.Length == 0)
		{
			this._builder.Notify(
				$"Tianzheng object {dxfClass.DxfName} {entity.Handle} carries no data to read",
				NotificationType.Warning);

			return template;
		}

		//The handle section is where the version specific paths put the handles reader: the field
		//this used to read is only filled in for R2010 and later, and an AC1021 drawing positions
		//it from the object's own end-of-data long instead.
		long handleOffset = this._handlesReader.PositionInBits() - payloadStart;

		TchTextReader.Read(entity, payload, handleOffset);

		return template;
	}
}