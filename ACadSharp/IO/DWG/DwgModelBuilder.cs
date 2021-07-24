#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgModelBuilder
	{
		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public DwgBlockCtrlObjectTemplate BlockControlTemplate { get; set; }

		public List<DwgBlockTemplate> BlockHeaders { get; } = new List<DwgBlockTemplate>();
		public List<ICadObjectBuilder> Blocks { get; } = new List<ICadObjectBuilder>();

		public DwgTableTemplate<AppId> AppIds { get; set; }

		public Dictionary<ulong, DwgTemplate> Templates { get; } = new Dictionary<ulong, DwgTemplate>();
		public Dictionary<ulong, CadObject> ObjectsMap { get; } = new Dictionary<ulong, CadObject>();

		public CadDocument DocumentToBuild { get; }

		public DwgModelBuilder(CadDocument document)
		{
			DocumentToBuild = document;
		}

		public void BuildModelBase()
		{
			foreach (DwgBlockTemplate header in BlockHeaders)
				header.Build(this);

			foreach (ICadObjectBuilder block in Blocks)
				block.Build(this);

			if (BlockControlTemplate != null)
				BlockControlTemplate.Build(this);

			if (AppIds != null)
				AppIds.Build(this);

			if (HeaderHandles.BYLAYER != null && TryGetCadObject(HeaderHandles.BYLAYER.Value, out LineType lineType))
				DocumentToBuild.LineTypes.Add(lineType);

			if (HeaderHandles.BYBLOCK != null && TryGetCadObject(HeaderHandles.BYBLOCK.Value, out LineType byBlock))
				DocumentToBuild.LineTypes.Add(byBlock);

			if (HeaderHandles.CONTINUOUS != null && TryGetCadObject(HeaderHandles.CONTINUOUS.Value, out LineType continuous))
				DocumentToBuild.LineTypes.Add(continuous);

			//foreach (ICadObjectBuilder item in this.DictionaryBuilders)
			//	item.Build(this);

			//foreach (ICadObjectBuilder item in this.DictionaryChildBuilders)
			//	item.Build(this);

			if (HeaderHandles.DIMSTYLE != null && TryGetCadObject(HeaderHandles.DIMSTYLE.Value, out DimensionStyle dstyle))
				DocumentToBuild.DimensionStyles.Add(dstyle);

			//Build all the objects in the document
			foreach (ICadObjectBuilder item in Templates.Values)
				item.Build(this);

			//foreach (var obj in this.ViewportTemplates)
			//	obj.Build(this);

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

		public bool TryGetCadObject<T>(ulong handle, out T value) where T : CadObject
		{
			if (ObjectsMap.TryGetValue(handle, out CadObject cadObject))
			{
				if (cadObject is T)
				{
					value = (T)cadObject;
					return true;
				}
			}

			value = null;
			return false;
		}

		public ICadObjectBuilder GetObjectBuilder(ulong handle)
		{
			if (Templates.TryGetValue(handle, out DwgTemplate builder))
			{
				return builder;
			}

			return null;
		}

		[Obsolete]
		public void BuildObjects()
		{
			foreach (var item in Templates.Values)
			{
				item.Build(this);
			}
		}
	}
}
