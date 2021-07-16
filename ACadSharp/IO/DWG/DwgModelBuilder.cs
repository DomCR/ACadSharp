#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgModelBuilder
	{
		public DwgBlockCtrlObjectTemplate BlockControlTemplate { get; set; }

		public List<DwgBlockTemplate> BlockHeaders { get; } = new List<DwgBlockTemplate>();

		public DwgHeaderHandlesCollection HeaderHandles { get; set; }
		public List<DwgTemplate> Templates { get; } = new List<DwgTemplate>();
		public Dictionary<ulong, CadObject> ObjectsMap { get; } = new Dictionary<ulong, CadObject>();

		public void BuildModelBase(CadDocument document)
		{


			throw new NotImplementedException();
		}

		public virtual void BuildHeaders()
		{
			foreach (DwgBlockTemplate item in this.BlockHeaders)
				item.Build(this);
		}

		public CadObject GetCadObject(ulong handle)
		{
			if (ObjectsMap.TryGetValue(handle, out CadObject cadObject))
				return cadObject;
			else
				return null;
		}

		public T GetCadObject<T>(ulong handle) where T : CadObject
		{
			if (ObjectsMap.TryGetValue(handle, out CadObject cadObject))
			{
				if (cadObject is T)
					return (T)cadObject;
			}

			return null;
		}

		public void BuildObjects()
		{
			foreach (var item in Templates)
			{
				item.Build(this);
			}
		}
	}
}
