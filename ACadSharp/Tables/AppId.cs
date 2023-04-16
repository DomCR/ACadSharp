using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

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
		/// <summary>
		/// Default application registry name.
		/// </summary>
		public const string DefaultName = "ACAD";

		public static AppId Default { get { return new AppId(DefaultName); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.APPID;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableAppId;

		internal AppId() : base() { }

		public AppId(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "App id must have a name.");
		}

		public override TableEntry Clone()
		{
			return (TableEntry)MemberwiseClone();

			AppId clone = new AppId(this.Name);
			this.mapClone(clone);
			return clone;
		}
	}
}
