using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="ModelerGeometry"/> entity.
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.ModelerGeometry)]
	public abstract partial class ModelerGeometry : Entity
	{
		public XYZ Point { get; set; }

		public List<Silhouette> Silhouettes { get; } = new();

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ModelerGeometry;

		public List<Wire> Wires { get; } = new();

		[DxfCodeValue(2)]
		internal Guid Guid { get; set; }

		/// <summary>
		/// Gets or sets the modeler format version used in the drawing.
		/// </summary>
		[DxfCodeValue(70)]
		public short ModelerFormatVersion { get; set; }

		[DxfCodeValue(1)]
		//[DxfCodeValue(3)]
		public StringBuilder ProprietaryData { get; } = new();

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
		/// Flag that indicates if <see cref="AcisData"/> contains a binary SAB
		/// payload instead of SAT text.
		/// </summary>
		public bool IsBinaryAcisData
		{
			get
			{
				if (this.AcisData == null || this.AcisData.Length < _asmBinarySignature.Length)
				{
					return false;
				}

				return startsWith(this.AcisData, _acisBinarySignature)
					|| startsWith(this.AcisData, _asmBinarySignature);
			}
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

		//Signature that marks the start of a binary SAB payload
		private static readonly byte[] _acisBinarySignature = Encoding.ASCII.GetBytes("ACIS BinaryFile");

		private static readonly byte[] _asmBinarySignature = Encoding.ASCII.GetBytes("ASM BinaryFile");

		private static bool startsWith(byte[] data, byte[] signature)
		{
			if (data.Length < signature.Length) return false;
			for (int i = 0; i < signature.Length; i++)
				if (data[i] != signature[i]) return false;
			return true;
		}

		internal bool? ModelerDataFlag { get; set; }
		internal List<(int ArrayIndex, int AbsoluteReference, ulong MaterialHandle)> MaterialMappings { get; } = new();
		internal bool RevisionGuidPresent { get; set; }
		internal int RevisionMajor { get; set; }
		internal short RevisionMinor1 { get; set; }
		internal short RevisionMinor2 { get; set; }
		internal byte[] RevisionBytes { get; set; } = new byte[8];
		internal int RevisionEndMarker { get; set; }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}