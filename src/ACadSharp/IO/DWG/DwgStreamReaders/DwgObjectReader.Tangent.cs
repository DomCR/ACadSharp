using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Entities.Tangent;
using ACadSharp.IO.DWG.Tangent;
using ACadSharp.IO.Templates;
using CSMath;
using System;

namespace ACadSharp.IO.DWG
{
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
		/// Whether an unlisted class is one of Tianzheng's.
		/// </summary>
		/// <remarks>
		/// By the DXF name, which every one of them begins with <c>TCH_</c>, and by the application
		/// name the class registers under, which does too. In the samples both hold and nothing else
		/// in the file shares either.
		/// </remarks>
		private static bool isTangent(DxfClass dxfClass)
		{
			return dxfClass.DxfName.StartsWith("TCH_", StringComparison.Ordinal)
				|| (dxfClass.ApplicationName?.StartsWith("TCH_", StringComparison.Ordinal) ?? false);
		}

		/// <summary>
		/// Reads a Tianzheng entity.
		/// </summary>
		/// <remarks>
		/// The type follows how the object is read, not which class it is: a class that writes text
		/// becomes a <see cref="TchText"/>, and anything else is kept whole as an
		/// <see cref="UnknownEntity"/> until a reader for its class exists. The class itself travels
		/// on the entity either way.
		/// </remarks>
		private CadTemplate readTangentEntity(DxfClass dxfClass)
		{
			if (TchText.WritesText(dxfClass.DxfName))
			{
				return this.readTangentText(dxfClass);
			}

			return this.readUnknownEntity(dxfClass);
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
			CadTchEntityTemplate template = new CadTchEntityTemplate(entity);

			this.readCommonEntityData(template);

			//The class specific fields have no reader in the filer, so the bytes are taken and what
			//can be read from them is read here rather than by the entity.
			long payloadStart = this._objectReader.PositionInBits();

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
}
