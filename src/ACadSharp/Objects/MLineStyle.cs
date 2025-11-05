using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="MLineStyle"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectMLineStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLineStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectMLineStyle)]
	[DxfSubClass(DxfSubclassMarker.MLineStyle)]
	public partial class MLineStyle : NonGraphicalObject
	{
		/// <summary>
		/// Gets the default multiline style with predefined settings.
		/// </summary>
		/// <remarks>This property provides a preconfigured instance of <see cref="MLineStyle"/> that can be used as a
		/// baseline for creating multiline styles. The default configuration includes two elements with offsets of 0.5 and
		/// -0.5, respectively.</remarks>
		public static MLineStyle Default
		{
			get
			{
				var def = new MLineStyle(DefaultName);
				def.StartAngle = MathHelper.HalfPI;
				def.EndAngle = MathHelper.HalfPI;
				def.AddElement(new Element
				{
					LineType = LineType.ByLayer,
					Offset = 0.5
				});
				def.AddElement(new Element
				{
					LineType = LineType.ByLayer,
					Offset = -0.5
				});
				return def;
			}
		}

		/// <summary>
		/// Style description
		/// </summary>
		/// <value>
		/// 255 characters maximum
		/// </value>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		/// <summary>
		/// Elements in the style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 71)]
		public IEnumerable<Element> Elements => this._elements;

		/// <summary>
		/// End angle
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 52)]
		public double EndAngle { get; set; } = System.Math.PI / 2;

		/// <summary>
		/// Fill color
		/// </summary>
		[DxfCodeValue(62)]
		public Color FillColor { get; set; } = Color.ByLayer;

		/// <summary>
		/// Multi line style flags
		/// </summary>
		[DxfCodeValue(70)]
		public MLineStyleFlags Flags { get; set; }

		/// <summary>
		/// Mline style name
		/// </summary>
		[DxfCodeValue(2)]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectMLineStyle;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.MLINESTYLE;

		/// <summary>
		/// Start angle
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double StartAngle { get; set; } = System.Math.PI / 2;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MLineStyle;

		/// <summary>
		/// Default multiline style name
		/// </summary>
		public const string DefaultName = "Standard";

		private List<Element> _elements = new List<Element>();

		public MLineStyle(string name)
		{
			this.Name = name;
		}

		internal MLineStyle()
		{
		}

		/// <summary>
		/// Adds a segment to the current line style.
		/// </summary>
		/// <remarks>The method associates the specified element with the current line style and updates its line type
		/// using the document's line type collection, if available.</remarks>
		/// <param name="element">The element to add as a segment. The element must not already belong to another line style.</param>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="element"/> is already assigned to another line style.</exception>
		public void AddElement(Element element)
		{
			if (element.Owner != null)
				throw new ArgumentException($"Element already assigned to a MLineStyle: {element.Owner.Name}");

			element.LineType = CadObject.updateCollection(element.LineType, this.Document?.LineTypes);
			element.Owner = this;
			this._elements.Add(element);
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			MLineStyle clone = (MLineStyle)base.Clone();

			clone._elements = new List<Element>();
			foreach (var element in this._elements)
			{
				clone.AddElement(element.Clone());
			}

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			foreach (var item in this._elements.Where(s => s.LineType != null))
			{
				item.AssignDocument(doc);
			}

			doc.TextStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.TextStyles.OnRemove -= this.tableOnRemove;

			foreach (var item in this._elements.Where(s => s.LineType != null))
			{
				item.UnassignDocument();
			}

			base.UnassignDocument();
		}

		protected void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is LineType style)
			{
				foreach (var item in this._elements.Where(s => s.LineType != null))
				{
					if (item.LineType == style)
					{
						item.LineType = null;
					}
				}
			}
		}
	}
}