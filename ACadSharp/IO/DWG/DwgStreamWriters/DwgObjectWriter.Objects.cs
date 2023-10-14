using ACadSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
		private void writeObjects()
		{
			while (this._objects.Any())
			{
				CadObject obj = this._objects.Dequeue();

				this.writeObject(obj);
			}
		}

		private void writeObject(CadObject obj)
		{
			this.writeCommonNonEntityData(obj);

			switch (obj)
			{
				case CadDictionary dictionary:
					this.writeDictionary(dictionary);
					break;
				case XRecrod recrod:
					this.xRecord(recrod);
					break;
				default:
					throw new NotImplementedException($"Object not implemented : {obj.GetType().FullName}");
			}

			this.registerObject(obj);
		}

		private void writeDictionary(CadDictionary dictionary)
		{
			//Common:
			//Numitems L number of dictonary items
			this._writer.WriteBitLong(dictionary.Count());

			//R14 Only:
			if (this._version == ACadVersion.AC1014)
			{
				//Unknown R14 RC Unknown R14 byte, has always been 0
				this._writer.WriteByte(0);
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				//Cloning flag BS 281
				this._writer.WriteBitShort((short)dictionary.ClonningFlags);
				this._writer.WriteByte((byte)(dictionary.HardOwnerFlag ? 1u : 0u));
			}

			//Common:
			foreach (var name in dictionary.EntryNames)
			{
				this._writer.WriteVariableText(name);
			}

			foreach (var handle in dictionary.EntryHandles)
			{
				this._writer.HandleReference(DwgReferenceType.SoftOwnership, handle);
			}

			this.addEntriesToWriter(dictionary);
		}

		private void addEntriesToWriter(CadDictionary dictionary)
		{
			foreach (CadObject e in dictionary)
			{
				this._objects.Enqueue(e);
			}
		}

		private void writeXRecrod(XRecrod recrod)
		{

		}
	}
}
