using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects
{
	public class ProxyObject : NonGraphicalObject, IProxy
	{
		[DxfCodeValue(91)]
		public int ClassId { get { return this.DxfClass.ItemClassId; } }

		public DxfClass DxfClass { get; set; }

		public int MaintenanceVersion { get; set; }

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

		//330 or 340
		//or 350 or 360
		//An object ID(multiple entries can appear) (optional)

		//94 0 (indicates end of object ID section)

		//95 Object drawing format when it becomes a proxy(a 32-bit unsigned integer):
		//Low word is AcDbDwgVersion
		//High word is MaintenanceReleaseVersion

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ProxyObject;

		public ACadVersion Version { get; set; }
	}
}