using System;

namespace ACadSharp
{
	/// <summary>
	/// Represents the transparency for the graphical objects.
	/// </summary>
	public struct Transparency
	{
		/// <summary>
		/// Gets the ByLayer transparency.
		/// </summary>
		public static Transparency ByLayer { get { return new Transparency(-1); } }

		/// <summary>
		/// Gets the ByBlock transparency.
		/// </summary>
		public static Transparency ByBlock { get { return new Transparency(100); } }

		/// <summary>
		/// Gets the Opaque transparency.
		/// </summary>
		public static Transparency Opaque { get { return new Transparency(0); } }

		/// <summary>
		/// Defines if the transparency is defined by layer.
		/// </summary>
		public bool IsByLayer
		{
			get { return _value == -1; }
		}

		/// <summary>
		/// Defines if the transparency is defined by block.
		/// </summary>
		public bool IsByBlock
		{
			get { return _value == 100; }
		}

		/// <summary>
		/// Gets or sets the transparency value.
		/// </summary>
		/// <remarks>
		/// Transparency values must be in range from 0 (opaque) to 90 (transparent), the reserved values -1 and 100 represents ByLayer and ByBlock.
		/// </remarks>
		public short Value
		{
			get { return _value; }
			set
			{
				if (value == -1)
				{
					_value = value;
					return;
				}

				if (value == 100)
				{
					_value = value;
					return;
				}

				if (value < 0 || value > 90)
					throw new ArgumentOutOfRangeException(nameof(value), value, "Transparency must be in range from 0 to 90.");

				_value = value;
			}
		}

		private short _value;

		/// <summary>
		/// Initializes a new instance of the Transparency struct.
		/// </summary>
		/// <param name="value">Alpha value range from 0 to 90.</param>
		/// <remarks>
		/// Transparency values must be in range from 0 (opaque) to 90 (transparent), the reserved values -1 and 100 represents ByLayer and ByBlock.
		/// </remarks>
		public Transparency(short value)
		{
			_value = -1;
			this.Value = value;
		}

		/// <summary>
		/// Gets the alpha value of a transperency.
		/// </summary>
		/// <param name="transparency"></param>
		/// <returns></returns>
		public static int ToAlphaValue(Transparency transparency)
		{
			byte alpha = (byte)(255 * (100 - transparency.Value) / 100.0);
			byte[] bytes = transparency.IsByBlock ? new byte[] { 0, 0, 0, 1 } : new byte[] { alpha, 0, 0, 2 };
			return BitConverter.ToInt32(bytes, 0);
		}

		/// <summary>
		/// Gets the transparency from a transparency value
		/// </summary>
		/// <param name="value">A transparency value</param>
		/// <returns>A <see cref="Transparency"></see></returns>
		public static Transparency FromAlphaValue(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			short alpha = (short)(100 - (bytes[0] / 255.0) * 100);

			if (alpha == -1)
			{
				return ByLayer;
			}

			if (alpha == 100)
			{
				return ByBlock;
			}

			if (alpha < 0)
			{
				return new Transparency(0);
			}

			if (alpha > 90)
			{
				return new Transparency(90);
			}

			return new Transparency(alpha);
		}
	}
}
