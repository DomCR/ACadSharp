using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACadSharp.IO.DWG
{
	/*
	 * WARNING: Reverse method from DwgReader.readHandles() simplified
	 */
	internal class DwgHandleWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.Handles;

		private MemoryStream _stream;

		private Dictionary<ulong, long> _handleMap;

		/// <param name="version"></param>
		/// <param name="stream"></param>
		/// <param name="map"></param>
		public DwgHandleWriter(ACadVersion version, MemoryStream stream, Dictionary<ulong, long> map) : base(version)
		{
			this._stream = stream;

#if NET48_OR_GREATER
			foreach (var item in map.OrderBy(o => o.Key))
			{
				this._handleMap.Add(item.Key, item.Value);
			}
#else
			this._handleMap = new Dictionary<ulong, long>(map.OrderBy(o => o.Key));
#endif
		}

		/// <param name="sectionOffset">For R18 the offset is relative, for earlier is absolute</param>
		public void Write(int sectionOffset = 0)
		{
			byte[] array = new byte[10];
			byte[] array2 = new byte[5];

			ulong offset = 0uL;
			long initialLoc = 0L;

			long lastPosition = this._stream.Position;

			this._stream.WriteByte(0);
			this._stream.WriteByte(0);

			foreach (var pair in this._handleMap)
			{
				ulong handleOff = pair.Key - offset;
				long lastLoc = (long)pair.Value + sectionOffset;
				long locDiff = lastLoc - initialLoc;

				int offsetSize = modularShortToValue(handleOff, array);
				int locSize = signedModularShortToValue((int)locDiff, array2);

				if (this._stream.Position - lastPosition + (offsetSize + locSize) > 2032)
				{
					this.processPosition(lastPosition);
					offset = 0uL;
					initialLoc = 0L;
					lastPosition = this._stream.Position;
					this._stream.WriteByte(0);
					this._stream.WriteByte(0);
					offset = 0uL;
					initialLoc = 0L;
					handleOff = pair.Key - offset;

					if (handleOff == 0)
					{
						throw new System.Exception();
					}

					locDiff = lastLoc - initialLoc;
					offsetSize = modularShortToValue(handleOff, array);
					locSize = signedModularShortToValue((int)locDiff, array2);
				}

				this._stream.Write(array, 0, offsetSize);
				this._stream.Write(array2, 0, locSize);
				offset = pair.Key;
				initialLoc = lastLoc;
			}

			this.processPosition(lastPosition);
			lastPosition = this._stream.Position;
			this._stream.WriteByte(0);
			this._stream.WriteByte(0);
			this.processPosition(lastPosition);
		}

		private int modularShortToValue(ulong value, byte[] arr)
		{
			int i = 0;
			while (value >= 0b10000000)
			{
				arr[i] = (byte)((value & 0b1111111) | 0b10000000);
				i++;
				value >>= 7;
			}
			arr[i] = (byte)value;
			return i + 1;
		}

		private int signedModularShortToValue(int value, byte[] arr)
		{
			int i = 0;
			if (value < 0)
			{
				for (value = -value; value >= 64; value >>= 7)
				{
					arr[i] = (byte)(((uint)value & 0x7Fu) | 0x80u);
					i++;
				}
				arr[i] = (byte)((uint)value | 0x40u);
				return i + 1;
			}

			while (value >= 0b1000000)
			{
				arr[i] = (byte)(((uint)value & 0x7Fu) | 0x80u);
				i++;
				value >>= 7;
			}

			arr[i] = (byte)value;
			return i + 1;
		}

		private void processPosition(long pos)
		{
			ushort diff = (ushort)(this._stream.Position - pos);
			long streamPos = this._stream.Position;
			this._stream.Position = pos;
			this._stream.WriteByte((byte)(diff >> 8));
			this._stream.WriteByte((byte)(diff & 0b11111111));
			this._stream.Position = streamPos;

			ushort crc = CRC8StreamHandler.GetCRCValue(0xC0C1, this._stream.GetBuffer(), pos, this._stream.Length - pos);
			this._stream.WriteByte((byte)(crc >> 8));
			this._stream.WriteByte((byte)(crc & 0b11111111));
		}
	}
}
