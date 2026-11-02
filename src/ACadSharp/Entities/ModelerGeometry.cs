using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="ModelerGeometry"/> entity.
/// </summary>
[DxfSubClass(DxfSubclassMarker.ModelerGeometry)]
public abstract partial class ModelerGeometry : Entity
{
	/// <summary>
	/// Raw ACIS payload that describes the geometry of this entity.
	/// </summary>
	/// <remarks>
	/// The payload is binary SAB when it starts with the "ACIS BinaryFile" signature
	/// (check <see cref="IsBinaryAcisData"/>), plain SAT text bytes otherwise.
	/// In R2013+ files the modeler geometry is stored apart from the entity
	/// (ACDSDATA section in DXF, AcDs data section in DWG) and this property is
	/// the only carrier of the geometry; older versions embed it in the entity
	/// itself.
	/// </remarks>
	public byte[] AcisData { get; set; }

	/// <summary>
	/// Gets or sets the globally unique identifier.
	/// </summary>
	[DxfCodeValue(2)]
	public Guid Guid { get; set; }

	/// <summary>
	/// Gets a value indicating whether any wireframe data is available.
	/// </summary>
	public bool HasWireframeData
	{
		get
		{
			return this.Wires.Count > 0 || this.Silhouettes.Count > 0;
		}
	}

	/// <summary>
	/// Flag that indicates if <see cref="AcisData"/> contains a binary SAB
	/// payload instead of SAT text.
	/// </summary>
	public bool IsBinaryAcisData
	{
		get
		{
			if (this.AcisData == null || this.AcisData.Length < _acisBinarySignature.Length)
			{
				return false;
			}

			for (int i = 0; i < _acisBinarySignature.Length; i++)
			{
				if (this.AcisData[i] != _acisBinarySignature[i])
				{
					return false;
				}
			}

			return true;
		}
	}

	/// <summary>
	/// Gets or sets the modeler format version used in the drawing.
	/// </summary>
	[DxfCodeValue(70)]
	public short ModelerFormatVersion { get; set; } = 1;

	/// <summary>
	/// Gets or sets the point in 3D space.
	/// </summary>
	public XYZ Point { get; set; }

	[DxfCodeValue(1)]
	//[DxfCodeValue(3)]
	public StringBuilder ProprietaryData { get; } = new();

	/// <summary>
	/// Gets the collection of silhouettes.
	/// </summary>
	/// <remarks>The returned list is mutable; add or remove items to modify the collection.</remarks>
	public List<Silhouette> Silhouettes { get; } = new();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.ModelerGeometry;

	/// <summary>
	/// Gets the collection of wires.
	/// </summary>
	public List<Wire> Wires { get; } = new();

	//Signature that marks the start of a binary SAB payload
	private static readonly byte[] _acisBinarySignature = Encoding.ASCII.GetBytes("ACIS BinaryFile");

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
	}

	/// <summary>
	/// Gets the SAT text carried by <see cref="AcisData"/>.
	/// </summary>
	/// <returns>The SAT content as a string, or null when the payload is empty or binary SAB.</returns>
	public string GetAcisText()
	{
		if (this.AcisData == null || this.AcisData.Length == 0 || this.IsBinaryAcisData)
		{
			return null;
		}

		return Encoding.ASCII.GetString(this.AcisData);
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		return BoundingBox.Null;
	}
}