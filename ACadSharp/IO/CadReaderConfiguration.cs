using ACadSharp.IO;

namespace ACadSharp.IO
{
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
		/// The reader will try to read and add to the document all <see cref="ObjectType.UNLISTED"/> that are linked to a <see cref="Classes. DxfClass"/> 
		/// which may be a proxy or an entity that is not yet supported by ACadSharp, default value is set to false.
		/// </summary>
		/// <remarks>
		/// These entities do not contain any geometric information and will be ignored by the writers
		/// </remarks>
		public bool KeepUnknownEntities { get; set; } = false;
	}
}
