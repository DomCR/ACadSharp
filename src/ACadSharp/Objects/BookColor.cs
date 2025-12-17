using ACadSharp.Attributes;
using CSUtilities.Extensions;
using System;
using System.Drawing;
using System.Linq;

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

		/// <summary>
		/// Returns the full name of the color, following the structure BookName$ColorName,
		/// if when set doesn't follow this structure only the name will be changed.
		/// </summary>
		/// <inheritdoc/>
		public override string Name
		{
			get
			{
				if (this.ColorName.IsNullOrEmpty())
				{
					return string.Empty;
				}
				else
				{
					return $"{this.BookName}${this.ColorName}";
				}
			}
			set
			{
				if (value.Contains('$'))
				{
					base.Name = value;

					this.BookName = value.Split('$').First();
					this.ColorName = value.Split('$').Last();
				}
				else
				{
					this.ColorName = value;
				}
			}
		}

		/// <summary>
		/// Color name.
		/// </summary>
		public string ColorName { get; set; }

		/// <summary>
		/// Book name where the color is stored.
		/// </summary>
		public string BookName { get; set; }

		[DxfCodeValue(62, 420)]
		public Color Color { get; set; }

		/// <inheritdoc/>
		public BookColor() : base() { }

		/// <summary>
		/// Initialize a <see cref="BookColor"/> with an specific name.
		/// </summary>
		/// <param name="name"></param>
		public BookColor(string name) : base(name)
		{
		}

		/// <summary>
		/// Initialize a <see cref="BookColor"/> with an specific book and color name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="bookName"></param>
		public BookColor(string name, string bookName) : base()
		{
			this.Name = name;
			this.BookName = bookName;
		}
	}
}
