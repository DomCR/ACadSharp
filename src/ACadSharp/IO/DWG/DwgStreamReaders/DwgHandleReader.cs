using CSUtilities.Converters;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgHandleReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.Handles; } }

		private IDwgStreamReader _sreader;

		public DwgHandleReader(IDwgStreamReader sreader, ACadVersion version) : base(version)
		{
			this._sreader = sreader;
		}

		public Dictionary<ulong, long> Read()
		{
			//Handle map, handle | loc
			Dictionary<ulong, long> objectMap = new Dictionary<ulong, long>();

			//Repeat until section size==2 (the last empty (except the CRC) section):
			while (true)
			{
				//Set the "last handle" to all 0 and the "last loc" to 0L;
				ulong lasthandle = 0;
				long lastloc = 0;

				//Short: size of this section. Note this is in BIGENDIAN order (MSB first)
				int size = _sreader.ReadShort<BigEndianConverter>();

				if (size == 2)
					break;

				long startPos = _sreader.Position;
				int maxSectionOffset = size - 2;
				//Note that each section is cut off at a maximum length of 2032.
				if (maxSectionOffset > 2032)
					maxSectionOffset = 2032;

				long lastPosition = startPos + maxSectionOffset;

				//Repeat until out of data for this section:
				while (_sreader.Position < lastPosition)
				{
					//offset of this handle from last handle as modular char.
					ulong offset = _sreader.ReadModularChar();
					lasthandle += offset;

					//offset of location in file from last loc as modular char. (note
					//that location offsets can be negative, if the terminating byte
					//has the 4 bit set).
					lastloc += _sreader.ReadSignedModularChar();

					if (offset > 0)
					{
						objectMap[lasthandle] = lastloc;
					}
					else
					{
						//0 offset, wrong reference
						this.notify($"Negative offset: {offset} for the handle: {lasthandle}", NotificationType.Warning);
					}
				}

				//CRC (most significant byte followed by least significant byte)
				uint crc = ((uint)_sreader.ReadByte() << 8) + _sreader.ReadByte();
			}

			return objectMap;
		}
	}
}