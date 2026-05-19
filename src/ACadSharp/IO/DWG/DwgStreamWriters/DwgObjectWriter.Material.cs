using ACadSharp.Objects;
using CSMath;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectWriter : DwgSectionIO
{
	// Mirror of DwgObjectReader.readMaterial. Field order and binary types are
	// kept symmetric with the reader so a Material round-tripped through
	// ACadSharp ends up with the same in-memory state.
	//
	// The reader stops at the end of the Refraction block; the writer emits the
	// same six "tail" fields AutoCAD writes after Refraction (translucence,
	// self_illumination, reflectivity, illumination_model, channel_flags, mode)
	// so the body length in the resulting DWG matches what AutoCAD and DWG
	// TrueView expect. The reader will still ignore those bytes because each
	// object knows its own size from the object map.

	private void writeMaterial(Material material)
	{
		// 1, 2 -- name + description as bit-text.
		this._writer.WriteVariableText(material.Name);
		this._writer.WriteVariableText(material.Description ?? string.Empty);

		// Ambient color block: method (RC byte), factor (BD), optional RGB (BL).
		this.writeMaterialColor(material.AmbientColorMethod, material.AmbientColorFactor, material.AmbientColor);

		// Diffuse color + diffuse map.
		this.writeMaterialColor(material.DiffuseColorMethod, material.DiffuseColorFactor, material.DiffuseColor);
		this.writeMaterialMap(
			material.DiffuseMapBlendFactor,
			(byte)material.DiffuseProjectionMethod,
			(byte)material.DiffuseTilingMethod,
			(byte)material.DiffuseAutoTransform,
			material.DiffuseMatrix,
			(byte)material.DiffuseMapSource,
			material.DiffuseMapFileName);

		// Specular color + specular map + gloss.
		this.writeMaterialColor(material.SpecularColorMethod, material.SpecularColorFactor, material.SpecularColor);
		this.writeMaterialMap(
			material.SpecularMapBlendFactor,
			(byte)material.SpecularProjectionMethod,
			(byte)material.SpecularTilingMethod,
			(byte)material.SpecularAutoTransform,
			material.SpecularMatrix,
			(byte)material.SpecularMapSource,
			material.SpecularMapFileName);
		this._writer.WriteBitDouble(material.SpecularGlossFactor);

		// Reflection map + opacity scalar.
		this.writeMaterialMap(
			material.ReflectionMapBlendFactor,
			(byte)material.ReflectionProjectionMethod,
			(byte)material.ReflectionTilingMethod,
			(byte)material.ReflectionAutoTransform,
			material.ReflectionMatrix,
			(byte)material.ReflectionMapSource,
			material.ReflectionMapFileName);
		this._writer.WriteBitDouble(material.Opacity);

		// Opacity map.
		this.writeMaterialMap(
			material.OpacityMapBlendFactor,
			(byte)material.OpacityProjectionMethod,
			(byte)material.OpacityTilingMethod,
			(byte)material.OpacityAutoTransform,
			material.OpacityMatrix,
			(byte)material.OpacityMapSource,
			material.OpacityMapFileName);

		// Bump map + refraction index scalar.
		this.writeMaterialMap(
			material.BumpMapBlendFactor,
			(byte)material.BumpProjectionMethod,
			(byte)material.BumpTilingMethod,
			(byte)material.BumpAutoTransform,
			material.BumpMatrix,
			(byte)material.BumpMapSource,
			material.BumpMapFileName);
		this._writer.WriteBitDouble(material.RefractionIndex);

		// Refraction map.
		this.writeMaterialMap(
			material.RefractionMapBlendFactor,
			(byte)material.RefractionProjectionMethod,
			(byte)material.RefractionTilingMethod,
			(byte)material.RefractionAutoTransform,
			material.RefractionMatrix,
			(byte)material.RefractionMapSource,
			material.RefractionMapFileName);

		// Tail block (R2007a+): values not read by readMaterial but emitted by
		// AutoCAD so the declared body size matches. Defaults sampled from an
		// AutoCAD-produced AC1027 reference: channel_flags = 0x7F, rest = 0.
		this._writer.WriteBitDouble(material.Translucence);
		this._writer.WriteBitDouble(0.0);                   // self_illumination
		this._writer.WriteBitDouble(material.Reflectivity);
		this._writer.WriteBitLong((int)material.IlluminationModel);
		this._writer.WriteBitLong((int)material.ChannelFlags);
		this._writer.WriteBitLong((int)material.Mode);
	}

	private void writeMaterialColor(ColorMethod method, double factor, Color rgb)
	{
		// readMaterial reads a single byte for method, then BD factor, then a
		// BL rgb only when method == Override. Encoder mirrors that.
		this._writer.WriteByte((byte)method);
		this._writer.WriteBitDouble(factor);
		if (method == ColorMethod.Override)
		{
			// True-color tag 0xc2 in the high byte marks the BL as a 24-bit
			// RGB true color instead of an index. ACadSharp's reader strips
			// this byte before constructing the Color so the round-trip works
			// either way; AutoCAD and DWG TrueView actually inspect the tag
			// when rendering. Writing 0 here was making materials come out
			// black under Realistic / Conceptual.
			byte[] arr = new byte[]
			{
				rgb.B,
				rgb.G,
				rgb.R,
				0b11000010
			};
			uint packed = CSUtilities.Converters.LittleEndianConverter.Instance.ToUInt32(arr);
			this._writer.WriteBitLong((int)packed);
		}
	}

	private void writeMaterialMap(double blendFactor, byte projection, byte tiling, byte autoTransform,
		Matrix4 transform, byte source, string fileName)
	{
		// readMaterial sequence: BD blendFactor, RC projection, RC tiling, RC
		// autoTransform, 16 BD matrix entries, RC source, optional T filename.
		this._writer.WriteBitDouble(blendFactor);
		this._writer.WriteByte(projection);
		this._writer.WriteByte(tiling);
		this._writer.WriteByte(autoTransform);
		this.writeMaterialMatrix(transform);
		this._writer.WriteByte(source);

		// Source enum: 0 = scene, 1 = file, 2 = procedural. Only the "file"
		// branch carries a follow-up text token in the binary stream.
		if (source == (byte)MapSource.UseImageFile)
		{
			this._writer.WriteVariableText(fileName ?? string.Empty);
		}
	}

	private void writeMaterialMatrix(Matrix4 m)
	{
		this._writer.WriteBitDouble(m.M00);
		this._writer.WriteBitDouble(m.M01);
		this._writer.WriteBitDouble(m.M02);
		this._writer.WriteBitDouble(m.M03);
		this._writer.WriteBitDouble(m.M10);
		this._writer.WriteBitDouble(m.M11);
		this._writer.WriteBitDouble(m.M12);
		this._writer.WriteBitDouble(m.M13);
		this._writer.WriteBitDouble(m.M20);
		this._writer.WriteBitDouble(m.M21);
		this._writer.WriteBitDouble(m.M22);
		this._writer.WriteBitDouble(m.M23);
		this._writer.WriteBitDouble(m.M30);
		this._writer.WriteBitDouble(m.M31);
		this._writer.WriteBitDouble(m.M32);
		this._writer.WriteBitDouble(m.M33);
	}
}
