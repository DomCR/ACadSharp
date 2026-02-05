using ACadSharp.Attributes;
using ACadSharp.Classes;
using System.IO;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="ProxyObject"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectProxyObject"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.ProxyObject"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectProxyObject)]
	[DxfSubClass(DxfSubclassMarker.ProxyObject)]
	public class ProxyObject : NonGraphicalObject, IProxy
	{
		/// <inheritdoc/>
		[DxfCodeValue(91)]
		public int ClassId { get { return this.DxfClass.ClassNumber; } }

		/// <summary>
		/// Object drawing format when it becomes a proxy: <br/>
		/// Low word is AcDbDwgVersion. <br/>
		/// High word is MaintenanceReleaseVersion.
		/// </summary>
		[DxfCodeValue(95)]
		public int DrawingFormat { get { return (int)this.Version | (this.MaintenanceVersion << 16); } }

		/// <inheritdoc/>
		public DxfClass DxfClass { get; set; }

		/// <inheritdoc/>
		public int MaintenanceVersion { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectProxyObject;

		/// <inheritdoc/>
		[DxfCodeValue(70)]
		public bool OriginalDataFormatDxf { get; set; }

		/// <inheritdoc/>
		/// <remarks>
		/// Always 499
		/// </remarks>
		[DxfCodeValue(90)]
		public int ProxyClassId { get; } = 499;

		//93 Size of entity data in bits
		//310 Binary entity data(multiple entries can appear) (optional)

		[DxfCodeValue(310)]
		public Stream BinaryData { get; set; }

		[DxfCodeValue(311)]
		public Stream Data { get; set; }

		//330 or 340
		//or 350 or 360
		//An object ID(multiple entries can appear) (optional)

		//94 0 (indicates end of object ID section)

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ProxyObject;

		/// <inheritdoc/>
		public ACadVersion Version { get; set; }
	}
}