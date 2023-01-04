#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;

namespace ACadSharp
{
    public readonly struct Color
    {
        private static readonly byte[][] _indexRgb = new byte[][]
        {
            new byte[] { 0, 0, 0 },
            new byte[] { 255, 0, 0 },
            new byte[] { 255, 255, 0 },
            new byte[] { 0, 255, 0 },
            new byte[] { 0, 255, 255 },
            new byte[] { 0, 0, 255 },
            new byte[] { 255, 0, 255 },
            new byte[] { 255, 255, 255 },
            new byte[] { 128, 128, 128 },
            new byte[] { 192, 192, 192 },
            new byte[] { 255, 0, 0 },
            new byte[] { 255, 127, 127 },
            new byte[] { 165, 0, 0 },
            new byte[] { 165, 82, 82 },
            new byte[] { 127, 0, 0 },
            new byte[] { 127, 63, 63 },
            new byte[] { 76, 0, 0 },
            new byte[] { 76, 38, 38 },
            new byte[] { 38, 0, 0 },
            new byte[] { 38, 19, 19 },
            new byte[] { 255, 63, 0 },
            new byte[] { 255, 159, 127 },
            new byte[] { 165, 41, 0 },
            new byte[] { 165, 103, 82 },
            new byte[] { 127, 31, 0 },
            new byte[] { 127, 79, 63 },
            new byte[] { 76, 19, 0 },
            new byte[] { 76, 47, 38 },
            new byte[] { 38, 9, 0 },
            new byte[] { 38, 28, 19 },
            new byte[] { 255, 127, 0 },
            new byte[] { 255, 191, 127 },
            new byte[] { 165, 82, 0 },
            new byte[] { 165, 124, 82 },
            new byte[] { 127, 63, 0 },
            new byte[] { 127, 95, 63 },
            new byte[] { 76, 38, 0 },
            new byte[] { 76, 57, 38 },
            new byte[] { 38, 19, 0 },
            new byte[] { 38, 28, 19 },
            new byte[] { 255, 191, 0 },
            new byte[] { 255, 223, 127 },
            new byte[] { 165, 124, 0 },
            new byte[] { 165, 145, 82 },
            new byte[] { 127, 95, 0 },
            new byte[] { 127, 111, 63 },
            new byte[] { 76, 57, 0 },
            new byte[] { 76, 66, 38 },
            new byte[] { 38, 28, 0 },
            new byte[] { 38, 33, 19 },
            new byte[] { 255, 255, 0 },
            new byte[] { 255, 255, 127 },
            new byte[] { 165, 165, 0 },
            new byte[] { 165, 165, 82 },
            new byte[] { 127, 127, 0 },
            new byte[] { 127, 127, 63 },
            new byte[] { 76, 76, 0 },
            new byte[] { 76, 76, 38 },
            new byte[] { 38, 38, 0 },
            new byte[] { 38, 38, 19 },
            new byte[] { 191, 255, 0 },
            new byte[] { 223, 255, 127 },
            new byte[] { 124, 165, 0 },
            new byte[] { 145, 165, 82 },
            new byte[] { 95, 127, 0 },
            new byte[] { 111, 127, 63 },
            new byte[] { 57, 76, 0 },
            new byte[] { 66, 76, 38 },
            new byte[] { 28, 38, 0 },
            new byte[] { 33, 38, 19 },
            new byte[] { 127, 255, 0 },
            new byte[] { 191, 255, 127 },
            new byte[] { 82, 165, 0 },
            new byte[] { 124, 165, 82 },
            new byte[] { 63, 127, 0 },
            new byte[] { 95, 127, 63 },
            new byte[] { 38, 76, 0 },
            new byte[] { 57, 76, 38 },
            new byte[] { 19, 38, 0 },
            new byte[] { 28, 38, 19 },
            new byte[] { 63, 255, 0 },
            new byte[] { 159, 255, 127 },
            new byte[] { 41, 165, 0 },
            new byte[] { 103, 165, 82 },
            new byte[] { 31, 127, 0 },
            new byte[] { 79, 127, 63 },
            new byte[] { 19, 76, 0 },
            new byte[] { 47, 76, 38 },
            new byte[] { 9, 38, 0 },
            new byte[] { 23, 38, 19 },
            new byte[] { 0, 255, 0 },
            new byte[] { 125, 255, 127 },
            new byte[] { 0, 165, 0 },
            new byte[] { 82, 165, 82 },
            new byte[] { 0, 127, 0 },
            new byte[] { 63, 127, 63 },
            new byte[] { 0, 76, 0 },
            new byte[] { 38, 76, 38 },
            new byte[] { 0, 38, 0 },
            new byte[] { 19, 38, 19 },
            new byte[] { 0, 255, 63 },
            new byte[] { 127, 255, 159 },
            new byte[] { 0, 165, 41 },
            new byte[] { 82, 165, 103 },
            new byte[] { 0, 127, 31 },
            new byte[] { 63, 127, 79 },
            new byte[] { 0, 76, 19 },
            new byte[] { 38, 76, 47 },
            new byte[] { 0, 38, 9 },
            new byte[] { 19, 88, 23 },
            new byte[] { 0, 255, 127 },
            new byte[] { 127, 255, 191 },
            new byte[] { 0, 165, 82 },
            new byte[] { 82, 165, 124 },
            new byte[] { 0, 127, 63 },
            new byte[] { 63, 127, 95 },
            new byte[] { 0, 76, 38 },
            new byte[] { 38, 76, 57 },
            new byte[] { 0, 38, 19 },
            new byte[] { 19, 88, 28 },
            new byte[] { 0, 255, 191 },
            new byte[] { 127, 255, 223 },
            new byte[] { 0, 165, 124 },
            new byte[] { 82, 165, 145 },
            new byte[] { 0, 127, 95 },
            new byte[] { 63, 127, 111 },
            new byte[] { 0, 76, 57 },
            new byte[] { 38, 76, 66 },
            new byte[] { 0, 38, 28 },
            new byte[] { 19, 88, 88 },
            new byte[] { 0, 255, 255 },
            new byte[] { 127, 255, 255 },
            new byte[] { 0, 165, 165 },
            new byte[] { 82, 165, 165 },
            new byte[] { 0, 127, 127 },
            new byte[] { 63, 127, 127 },
            new byte[] { 0, 76, 76 },
            new byte[] { 38, 76, 76 },
            new byte[] { 0, 38, 38 },
            new byte[] { 19, 88, 88 },
            new byte[] { 0, 191, 255 },
            new byte[] { 127, 223, 255 },
            new byte[] { 0, 124, 165 },
            new byte[] { 82, 145, 165 },
            new byte[] { 0, 95, 127 },
            new byte[] { 63, 111, 217 },
            new byte[] { 0, 57, 76 },
            new byte[] { 38, 66, 126 },
            new byte[] { 0, 28, 38 },
            new byte[] { 19, 88, 88 },
            new byte[] { 0, 127, 255 },
            new byte[] { 127, 191, 255 },
            new byte[] { 0, 82, 165 },
            new byte[] { 82, 124, 165 },
            new byte[] { 0, 63, 127 },
            new byte[] { 63, 95, 127 },
            new byte[] { 0, 38, 76 },
            new byte[] { 38, 57, 126 },
            new byte[] { 0, 19, 38 },
            new byte[] { 19, 28, 88 },
            new byte[] { 0, 63, 255 },
            new byte[] { 127, 159, 255 },
            new byte[] { 0, 41, 165 },
            new byte[] { 82, 103, 165 },
            new byte[] { 0, 31, 127 },
            new byte[] { 63, 79, 127 },
            new byte[] { 0, 19, 76 },
            new byte[] { 38, 47, 126 },
            new byte[] { 0, 9, 38 },
            new byte[] { 19, 23, 88 },
            new byte[] { 0, 0, 255 },
            new byte[] { 127, 127, 255 },
            new byte[] { 0, 0, 165 },
            new byte[] { 82, 82, 165 },
            new byte[] { 0, 0, 127 },
            new byte[] { 63, 63, 127 },
            new byte[] { 0, 0, 76 },
            new byte[] { 38, 38, 126 },
            new byte[] { 0, 0, 38 },
            new byte[] { 19, 19, 88 },
            new byte[] { 63, 0, 255 },
            new byte[] { 159, 127, 255 },
            new byte[] { 41, 0, 165 },
            new byte[] { 103, 82, 165 },
            new byte[] { 31, 0, 127 },
            new byte[] { 79, 63, 127 },
            new byte[] { 19, 0, 76 },
            new byte[] { 47, 38, 126 },
            new byte[] { 9, 0, 38 },
            new byte[] { 23, 19, 88 },
            new byte[] { 127, 0, 255 },
            new byte[] { 191, 127, 255 },
            new byte[] { 165, 0, 82 },
            new byte[] { 124, 82, 165 },
            new byte[] { 63, 0, 127 },
            new byte[] { 95, 63, 127 },
            new byte[] { 38, 0, 76 },
            new byte[] { 57, 38, 126 },
            new byte[] { 19, 0, 38 },
            new byte[] { 28, 19, 88 },
            new byte[] { 191, 0, 255 },
            new byte[] { 223, 127, 255 },
            new byte[] { 124, 0, 165 },
            new byte[] { 142, 82, 165 },
            new byte[] { 95, 0, 127 },
            new byte[] { 111, 63, 127 },
            new byte[] { 57, 0, 76 },
            new byte[] { 66, 38, 76 },
            new byte[] { 28, 0, 38 },
            new byte[] { 88, 19, 88 },
            new byte[] { 255, 0, 255 },
            new byte[] { 255, 127, 255 },
            new byte[] { 165, 0, 165 },
            new byte[] { 165, 82, 165 },
            new byte[] { 127, 0, 127 },
            new byte[] { 127, 63, 127 },
            new byte[] { 76, 0, 76 },
            new byte[] { 76, 38, 76 },
            new byte[] { 38, 0, 38 },
            new byte[] { 88, 19, 88 },
            new byte[] { 255, 0, 191 },
            new byte[] { 255, 127, 223 },
            new byte[] { 165, 0, 124 },
            new byte[] { 165, 82, 145 },
            new byte[] { 127, 0, 95 },
            new byte[] { 127, 63, 111 },
            new byte[] { 76, 0, 57 },
            new byte[] { 76, 38, 66 },
            new byte[] { 38, 0, 28 },
            new byte[] { 88, 19, 88 },
            new byte[] { 255, 0, 127 },
            new byte[] { 255, 127, 191 },
            new byte[] { 165, 0, 82 },
            new byte[] { 165, 82, 124 },
            new byte[] { 127, 0, 63 },
            new byte[] { 127, 63, 95 },
            new byte[] { 76, 0, 38 },
            new byte[] { 76, 38, 57 },
            new byte[] { 38, 0, 19 },
            new byte[] { 88, 19, 28 },
            new byte[] { 255, 0, 63 },
            new byte[] { 255, 127, 159 },
            new byte[] { 165, 0, 41 },
            new byte[] { 165, 82, 103 },
            new byte[] { 127, 0, 31 },
            new byte[] { 127, 63, 79 },
            new byte[] { 76, 0, 19 },
            new byte[] { 76, 38, 47 },
            new byte[] { 38, 0, 9 },
            new byte[] { 88, 19, 23 },
            new byte[] { 0, 0, 0 },
            new byte[] { 101, 101, 101 },
            new byte[] { 102, 102, 102 },
            new byte[] { 153, 153, 153 },
            new byte[] { 204, 204, 204 },
            new byte[] { 255, 255, 255 }
        };

        private const int MaxTrueColor = 1 << 24;
        public static Color ByLayer { get { return new Color(256); } }
        public static Color ByBlock { get { return new Color(0); } }

        /// <summary>
        /// Defines if the color is defined by layer.
        /// </summary>
        public bool IsByLayer
        {
            get { return this.Index == 256; }
        }

        /// <summary>
        /// Defines if the color is defined by block.
        /// </summary>
        public bool IsByBlock
        {
            get { return this.Index == 0; }
        }

        /// <summary>
        /// Indexed color.  If the color is stored as a true color, returns -1;
        /// </summary>
        public short Index => IsTrueColor ? (short)-1 : (short)_color;

        /// <summary>
        /// True color.  If the color is stored as an indexed color, returns -1;
        /// </summary>
        public int TrueColor => IsTrueColor ? _color ^ (1 << 30) : -1;

        /// <summary>
        /// True if the stored color is a true color.  False if the color is an indexed color.
        /// </summary>
        public bool IsTrueColor => _color >= (1 << 30);

        /// <summary>
        /// Represents the actual stored color.  Either a True Color or an indexed color.
        /// </summary>
        /// <remarks>
        /// If the number is >= 0, then the stored color is an indexed color ranging from 0 to 256.
        /// If the number is &lt; 0, then the stored color is a true color, less the negative sign.
        /// </remarks>
        private readonly int _color;
        
        /// <summary>
        /// Creates a new color out of an AutoCad indexed color.
        /// </summary>
        /// <param name="index">AutoCad index color</param>
        public Color(short index)
        {
            _color = index;
        }

        /// <summary>
        /// Creates a color out of RGB true color bytes.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public Color(byte r, byte g, byte b)
            : this(new[] { r, g, b })
        {
        }

        /// <summary>
        /// Creates a color out of a RGB true color byte array.
        /// </summary>
        /// <param name="rgb">Red Green Blue</param>
        public Color(byte[] rgb)
            : this(GetInt24(rgb))
        {

        }

        /// <summary>
        /// Creates a color out of a RGB true color.
        /// </summary>
        /// <param name="trueColor">True color int 32.</param>
        private Color(int trueColor)
        {
            if (trueColor < 0 || trueColor > MaxTrueColor)
                throw new ArgumentOutOfRangeException(nameof(trueColor), "True color must be a 24 bit color.");
                    
            // Shift to set the 30th bit indicating a true color.
            _color = trueColor | 1 << 30;
        }

        /// <summary>
        /// Creates a color out of a true color int32.
        /// </summary>
        /// <param name="color">True color int 32.</param>
        public static Color FromTrueColor(int color)
        {
            return new Color(color);
        }

        /// <summary>
        /// Approximates color from a true color RGB.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <returns>Approximate AutoCad RGB color.</returns>
        public static byte ApproxIndex(byte r, byte g, byte b)
        {
            var prevDist = -1;
            for (var i = 0; i < _indexRgb.Length; i++)
            {
                var dist = (r - _indexRgb[i][0]) + (g - _indexRgb[i][1]) + (b - _indexRgb[i][2]);
                if (dist == 0)
                    return (byte)i;

                if (dist < prevDist)
                {
                    prevDist = dist;
                    return (byte)i;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns the RGP color code which matches the passed indexed color.
        /// </summary>
        /// <returns>Approximate RGB color from AutoCAD's indexed color.</returns>
        public static ReadOnlySpan<byte> GetIndexRGB(int index)
        {
            return _indexRgb[index].AsSpan();
        }

        public ReadOnlySpan<byte> GetTrueColorRgb()
        {
            if (IsTrueColor)
            {
                return new ReadOnlySpan<byte>(BitConverter.GetBytes(_color), 0, 3);
            }

            return default;
        }

        public bool Equals(Color other)
        {
            return _color == other._color;
        }

        public override bool Equals(object obj)
        {
            return obj is Color other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _color;
        }
        
        public override string ToString()
        {
            if (IsTrueColor)
            {
                var parts = GetTrueColorRgb();
                return $"True Color RGB:{parts[0]},{parts[1]},{parts[2]}";
            }

            return $"Indexed Color:{Index}";

        }

        private static int GetInt24(byte[] array)
        {
            if (BitConverter.IsLittleEndian)
                return array[0] | array[1] << 8 | array[2] << 16;
            else
                return array[0] << 16 | array[1] << 8 | array[2];
        }
    }
}
