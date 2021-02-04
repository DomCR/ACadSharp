using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public struct Transparency
	{
		public static Transparency ByLayer { get { return new Transparency(-1); } }
		public static Transparency ByBlock { get { return new Transparency(100); } }
		public static Transparency Opaque { get { return new Transparency(0); } }

		/// <summary>
		/// Defines if the transparency is defined by layer.
		/// </summary>
		public bool IsByLayer
		{
			get { return m_transparency == -1; }
		}
		/// <summary>
		/// Defines if the transparency is defined by block.
		/// </summary>
		public bool IsByBlock
		{
			get { return m_transparency == 100; }
		}
		/// <summary>
		/// Gets or sets the transparency value.
		/// </summary>
		/// <remarks>
		/// Transparency values must be in range from 0 to 90, the reserved values -1 and 100 represents ByLayer and ByBlock.
		/// </remarks>
		public short Value
		{
			get { return m_transparency; }
			set
			{
				if (value == -1)
				{
					m_transparency = value;
					return;
				}

				if (value == 100)
				{
					m_transparency = value;
					return;
				}

				if (value < 0 || value > 90)
					throw new ArgumentOutOfRangeException(nameof(value), value, "Transparency must be in range from 0 to 90.");

				m_transparency = value;
			}
		}

		private short m_transparency;

		public Transparency(short value)
		{
			m_transparency = -1;
			Value = value;
		}
	}
}
