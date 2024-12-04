using ACadSharp.Attributes;
using System;
using System.Linq;
using System.Xml.Linq;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="BookColor"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDBColor"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DbColor"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDBColor)]
	[DxfSubClass(DxfSubclassMarker.DbColor)]
	public class BookColor : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDBColor;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DbColor;

		/// <inheritdoc/>
		public override string Name
		{
			get { return base.Name; }
			set
			{
				if (!value.Contains('$'))
				{
					throw new ArgumentException($"Invalid BookColor name: ({value}), a book color name has to separate the book name and the color name by the character '$'", nameof(value));
				}

				base.Name = value;
			}
		}

		/// <summary>
		/// Color name.
		/// </summary>
		public string ColorName { get { return this.Name.Split('$').Last(); } }

		/// <summary>
		/// Book name where the color is stored.
		/// </summary>
		public string BookName { get { return this.Name.Split('$').First(); } }

		[DxfCodeValue(62, 420)]
		public Color Color { get; set; }

		public BookColor() : base() { }

		public BookColor(string name) : base(name)
		{
		}
	}
}
