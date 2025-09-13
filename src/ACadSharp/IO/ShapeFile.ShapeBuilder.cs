using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO
{
	public partial class ShapeFile
	{
		internal class ShapeBuilder
		{
			public List<XY> CurrentLine
			{
				get
				{
					if (this.Polylines.Count == 0)
					{
						this.Polylines.Add(new List<XY>());
					}

					return this.Polylines.Last();
				}
			}

			public XY LastPt { get; set; }

			public bool NotStore { get; set; }

			public List<List<XY>> Polylines { get; } = new();

			internal bool DrawModeOn
			{
				get
				{
					return this._drawModeOn;
				}
				set
				{
					this._drawModeOn = value;
					this._isLine = false;
				}
			}

			private static readonly XY[] _vectors =
			{
				new XY(1.0, 0.0),
				new XY(1.0, 0.5),
				new XY(1.0, 1.0),
				new XY(0.5, 1.0),
				new XY(0.0, 1.0),
				new XY(-0.5, 1.0),
				new XY(-1.0, 1.0),
				new XY(-1.0, 0.5),
				new XY(-1.0, 0.0),
				new XY(-1.0, -0.5),
				new XY(-1.0, -1.0),
				new XY(-0.5, -1.0),
				new XY(0.0, -1.0),
				new XY(0.5, -1.0),
				new XY(1.0, -1.0),
				new XY(1.0, -0.5)
			};

			private readonly Stack<XY> _locationStack = new();

			private double _current = 1.0d;

			private bool _drawModeOn = true;

			private bool _isLine = false;

			public ShapeBuilder()
			{
			}

			public bool HasToStore()
			{
				bool result = !this.NotStore;
				this.NotStore = false;

				return result;
			}

			public int ProcessValue(int index, byte[] data)
			{
				byte code = data[index];
				switch (code)
				{
					case 0x0:
						// End of shape definition
						return 1;
					case 0x1:
						// Activate Draw mode(pen down)
						if (this.HasToStore())
						{
							this.DrawModeOn = true;
						}
						return 1;
					case 0x2:
						//Deactivate Draw mode (pen up)
						if (this.HasToStore())
						{
							this.DrawModeOn = false;
						}
						return 1;
					case 0x3:
						if (this.HasToStore())
						{
							//Divide vector lengths by next byte
							double div = (sbyte)data[index + 1];
							this._current /= div;
						}
						return 2;
					case 0x4:
						if (this.HasToStore())
						{
							//Multiply vector lengths by next byte
							double mult = (sbyte)data[index + 1];
							this._current *= mult;
						}
						return 2;
					case 0x5:
						if (this.HasToStore())
						{
							//Push current location onto stack
							this._locationStack.Push(this.LastPt);
						}
						return 1;
					case 0x6:
						if (this.HasToStore())
						{
							//Pop current location from stack
							if (this._locationStack.Count > 0)
							{
								this.LastPt = this._locationStack.Pop();
							}

							this._isLine = false;
						}
						return 1;
					case 0x7:
						//Draw subshape number given by next byte
						throw new NotImplementedException();
					case 0x8:
						if (this.HasToStore())
						{
							//X-Y displacement given by next two bytes
							var x = (sbyte)data[index + 1];
							var y = (sbyte)data[index + 2];

							this.drawLine(x, y);
						}
						return 3;
					case 0x9:
						//Multiple X-Y displacements, terminated (0,0)
						bool flag = this.HasToStore();
						int i;
						for (i = index + 1; data[i] != 0 || data[i + 1] != 0; i += 2)
						{
							if (flag)
							{
								this.drawLine((sbyte)data[i], (sbyte)data[i + 1]);
							}
						}
						return i - index + 2;
					case 0xA:
					//Octant arc defined by next two bytes
					case 0xB:
					//Fractional arc defined by next five bytes
					case 0xC:
					//Arc defined by X-Y displacement and bulge
					case 0xD:
					//Multiple bulge-specified arcs
					case 0xE:
						//Process next command only if vertical text
						throw new NotImplementedException();
					default:
						byte b = data[index];
						double num2 = (b >> 4) & 0xF;
						XY vector2D = _vectors[b & 0xF];
						if (this.HasToStore())
						{
							this.drawLine(num2 * vector2D.X, num2 * vector2D.Y);
						}
						return 1;
				}
			}

			private void createNewLine()
			{
				if (!this._isLine)
				{
					this._isLine = true;
					this.Polylines.Add(new List<XY>());
				}
			}

			private void drawLine(double x, double y)
			{
				var point2D = new XY(
				this.LastPt.X + this._current * x,
				this.LastPt.Y + this._current * y);

				if (this._drawModeOn)
				{
					this.createNewLine();
					this.CurrentLine.Add(point2D);
				}

				this.LastPt = point2D;
			}
		}
	}
}