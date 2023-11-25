using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp
{
	//Implement in CSMath
	public struct BoundingBox
	{
		public XYZ Min { get; set; }
		
		public XYZ Max { get; set; }

		/// <summary>
		/// Center of the box
		/// </summary>
		public XYZ Center
		{
			get
			{
				return Min + (Max - this.Min) * 0.5;
			}
		}
	}
}
