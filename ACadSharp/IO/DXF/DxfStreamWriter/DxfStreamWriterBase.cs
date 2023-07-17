namespace ACadSharp.IO.DXF
{
	internal abstract class DxfStreamWriterBase : IDxfStreamWriter
	{
		public bool WriteOptional { get; } = false;

		public void Write(DxfCode code, object value)
		{
			this.Write((int)code, value, null);
		}

		public void Write(DxfCode code, object value, DxfClassMap map)
		{
			this.Write((int)code, value, map);
		}

		public void Write(int code, object value)
		{
			this.Write(code, value, null);
		}

		public void WriteHandle(int code, CadObject value, DxfClassMap map)
		{
			if (value == null)
			{
				this.Write(code, 0, map);
			}
			else
			{
				this.Write(code, value.Handle, map);
			}
		}

		public void Write(int code, object value, DxfClassMap map)
		{
			if (map != null && map.DxfProperties.TryGetValue(code, out DxfProperty prop))
			{
				if (prop.ReferenceType.HasFlag(DxfReferenceType.Optional) && !WriteOptional)
				{
					return;
				}

				if (prop.ReferenceType.HasFlag(DxfReferenceType.IsAngle))
				{
					value = (double)value * MathUtils.RadToDeg;
				}
			}

			this.writeDxfCode(code);
			this.writeValue(code, value);
		}

		/// <inheritdoc/>
		public abstract void Dispose();

		public abstract void Flush();

		public abstract void Close();

		protected abstract void writeDxfCode(int code);

		protected abstract void writeValue(int code, object value);
	}
}
