using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Scale"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectScale"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Scale"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectScale)]
	[DxfSubClass(DxfSubclassMarker.Scale)]
	public class Scale : NonGraphicalObject
	{
		public const string DefaultName = "1:1";

		public static Scale Default { get { return new Scale { Name = DefaultName, PaperUnits = 1.0, DrawingUnits = 1.0, IsUnitScale = true }; } }

		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectScale;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Scale;

		/// <summary>
		/// Name
		/// </summary>
		[DxfCodeValue(300)]
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

		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(140)]
		public double PaperUnits { get; set; }

		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(141)]
		public double DrawingUnits { get; set; }

		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(290)]
		public bool IsUnitScale { get; set; }

		public double ScaleFactor => this.PaperUnits / this.DrawingUnits;

		public Scale() { }

		public Scale(string name) : base(name)
		{
		}

		public double ApplyTo(double value)
		{
			return value * ScaleFactor;
		}

		public T ApplyTo<T>(T value)
			where T : IVector, new()
		{
			T result = new();

			for (int i = 0; i < value.Dimension; i++)
			{
				result[i] = ApplyTo(value[i]);
			}

			return result;
		}
	}
}
