using ACadSharp.Attributes;
using System;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="AppId"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableAppId"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.ApplicationId"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableAppId)]
	[DxfSubClass(DxfSubclassMarker.ApplicationId)]
	public class AppId : TableEntry
	{
		public static AppId Default { get { return new AppId(DefaultName); } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableAppId;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.APPID;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ApplicationId;

		/// <summary>
		/// Application name for BlockRepETag.
		/// </summary>
		public const string BlockRepBTag = "AcDbBlockRepBTag";

		/// <summary>
		/// Application name for AcDbBlockRepETag.
		/// </summary>
		public const string BlockRepETag = "AcDbBlockRepETag";

		/// <summary>
		/// Default application registry name.
		/// </summary>
		public const string DefaultName = "ACAD";

		/// <inheritdoc/>
		public AppId(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "Application id must have a name.");
		}

		internal AppId() : base()
		{
		}
	}
}