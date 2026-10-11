using System.Collections.Generic;

namespace ACadSharp.IO
{
	/// <summary>
	/// Base configuration for all the <see cref="CadReaderBase{T}"/> classes.
	/// </summary>
	public abstract class CadReaderConfiguration
	{
		/// <summary>
		/// The reader will try to continue when an exception is thrown
		/// </summary>
		/// <remarks>
		/// The result file may be incomplete or with some objects missing due the error
		/// </remarks>
		public bool Failsafe { get; set; } = true;

		/// <summary>
		/// The reader will try to read and add to the document all <see cref="ObjectType.UNLISTED"/> that are linked to a <see cref="Classes.DxfClass"/> 
		/// which may be a proxy or an entity that is not yet supported by ACadSharp, default value is set to false.
		/// </summary>
		/// <remarks>
		/// These entities do not contain any geometric information and will be ignored by the writers.
		/// </remarks>
		public bool KeepUnknownEntities { get; set; } = false;

		/// <summary>
		/// The reader will try to read and add to the document all <see cref="ObjectType.UNLISTED"/> that are linked to a <see cref="Classes.DxfClass"/> 
		/// which may be a proxy or an NonGraphicalObject that is not yet supported by ACadSharp, default value is set to false.
		/// </summary>
		/// <remarks>
		/// These NonGraphicalObjects do not contain any information and will be ignored by the writers.
		/// </remarks>
		public bool KeepUnknownNonGraphicalObjects { get; set; } = false;

		/// <summary>
		/// Additional directories where the shape files (<c>.shx</c>) referenced by the
		/// document are searched.
		/// </summary>
		/// <remarks>
		/// A <see cref="Entities.Shape"/> is stored by name in DXF, but by number plus a
		/// reference to its shape file in DWG. The reader resolves the name against the
		/// <c>.shx</c> files of the shape file styles in the document; add the folder that
		/// holds the project shape files here when it is not a standard AutoCAD fonts
		/// folder nor the folder of the drawing itself.
		/// </remarks>
		public List<string> FontSearchDirectories { get; } = new List<string>();
	}
}
