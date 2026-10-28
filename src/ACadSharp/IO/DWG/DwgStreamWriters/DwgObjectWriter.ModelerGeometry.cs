using ACadSharp.Entities;
using ACadSharp.IO.DWG.DwgStreamWriters;
using CSMath;
using System;
using System.Linq;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectWriter
{
	private void writeModelerGeometry(ModelerGeometry geometry)
	{
		byte[] data = geometry.AcisData ?? Array.Empty<byte>();
		bool storedInAcDs = this.R2013Plus && data.Length > 0;
		short version = (short)(storedInAcDs || geometry.IsBinaryAcisData ? 2 : 1);
		if (!storedInAcDs) this._writer.WriteBit(data.Length == 0);
		if (data.Length > 0 && !storedInAcDs)
		{
			this._writer.WriteBit(geometry.ModelerDataFlag ?? version == 1);
			this._writer.WriteBitShort(version);
			if (version == 2)
			{
				this._writer.WriteBytes(data);
			}
			else
			{
				for (int offset = 0; offset < data.Length; offset += 4096)
				{
					byte[] block = new byte[Math.Min(4096, data.Length - offset)];
					Array.Copy(data, offset, block, 0, block.Length);
					this._writer.WriteBitLong(block.Length);
					this._writer.WriteBytes(AcisTextCodec.Decode(block));
				}
				this._writer.WriteBitLong(0);
			}
		}

		bool hasPoint = geometry.Point != XYZ.Zero;
		bool hasWireframe = hasPoint || geometry.Wires.Any() || geometry.Silhouettes.Any();
		this._writer.WriteBit(hasWireframe);
		if (hasWireframe)
		{
			this._writer.WriteBit(hasPoint);
			if (hasPoint) this._writer.Write3BitDouble(geometry.Point);
			this._writer.WriteBitLong(0);
			this._writer.WriteBit(geometry.Wires.Any());
			if (geometry.Wires.Any())
			{
				this._writer.WriteBitLong(geometry.Wires.Count);
				foreach (var wire in geometry.Wires) this.writeModelerGeometryWire(wire);
			}
			this._writer.WriteBitLong(geometry.Silhouettes.Count);
			foreach (var silhouette in geometry.Silhouettes)
			{
				this._writer.WriteBitLongLong(silhouette.ViewportId);
				this._writer.Write3BitDouble(silhouette.ViewportTarget);
				this._writer.Write3BitDouble(silhouette.ViewportDirectionFromTarget);
				this._writer.Write3BitDouble(silhouette.ViewportUpDirection);
				this._writer.WriteBit(silhouette.ViewportPerspective);
				this._writer.WriteBit(silhouette.Wires.Any());
				if (silhouette.Wires.Any())
				{
					this._writer.WriteBitLong(silhouette.Wires.Count);
					foreach (var wire in silhouette.Wires) this.writeModelerGeometryWire(wire);
				}
			}
		}
		this._writer.WriteBit(true);
		if (this.R2007Plus && version > 1)
		{
			this._writer.WriteBitLong(geometry.MaterialMappings.Count);
			foreach (var material in geometry.MaterialMappings)
			{
				this._writer.WriteBitLong(material.ArrayIndex);
				this._writer.WriteBitLong(material.AbsoluteReference);
				this._writer.HandleReference(DwgReferenceType.HardPointer, material.MaterialHandle);
			}
		}
		if (this.R2013Plus)
		{
			this._writer.WriteBit(geometry.RevisionGuidPresent || geometry.Guid != Guid.Empty);
			this.writeGuid(geometry.Guid);
			this._writer.WriteBitLong(geometry.RevisionEndMarker);
		}
	}

	private void writeModelerGeometryWire(ModelerGeometry.Wire wire)
	{
		this._writer.WriteByte(wire.Type);
		this._writer.WriteBitLong(wire.SelectionMarker);
		this._writer.WriteBitShort(wire.Color.GetApproxIndex());
		this._writer.WriteBitLong(wire.AcisIndex);
		this._writer.WriteBitLong(wire.Points.Count);
		foreach (var point in wire.Points) this._writer.Write3BitDouble(point);
		this._writer.WriteBit(wire.ApplyTransformPresent);
		if (!wire.ApplyTransformPresent) return;
		this._writer.Write3BitDouble(wire.XAxis);
		this._writer.Write3BitDouble(wire.YAxis);
		this._writer.Write3BitDouble(wire.ZAxis);
		this._writer.Write3BitDouble(wire.Translation);
		this._writer.WriteBitDouble(wire.Scale);
		this._writer.WriteBit(wire.HasRotation);
		this._writer.WriteBit(wire.HasReflection);
		this._writer.WriteBit(wire.HasShear);
	}
}
