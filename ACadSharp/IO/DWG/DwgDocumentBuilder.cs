using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder
	{
		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public Dictionary<ulong, DwgTemplate> Templates { get; } = new Dictionary<ulong, DwgTemplate>();

		public Dictionary<ulong, CadObject> ObjectsMap { get; } = new Dictionary<ulong, CadObject>();

		public CadDocument DocumentToBuild { get; }

		public NotificationEventHandler NotificationHandler { get; }

		public DwgDocumentBuilder(CadDocument document, NotificationEventHandler notification = null)
		{
			this.DocumentToBuild = document;
			this.NotificationHandler = notification;
		}

		public void BuildDocument()
		{
			if (this.HeaderHandles.BYLAYER.HasValue && this.TryGetCadObject(this.HeaderHandles.BYLAYER.Value, out LineType lineType))
				this.DocumentToBuild.LineTypes.Add(lineType);
			else
				throw new NotImplementedException();

			if (this.HeaderHandles.BYBLOCK.HasValue && this.TryGetCadObject(this.HeaderHandles.BYBLOCK.Value, out LineType byBlock))
				this.DocumentToBuild.LineTypes.Add(byBlock);
			else
				throw new NotImplementedException();

			if (this.HeaderHandles.CONTINUOUS.HasValue && this.TryGetCadObject(this.HeaderHandles.CONTINUOUS.Value, out LineType continuous))
				this.DocumentToBuild.LineTypes.Add(continuous);
			else
				throw new NotImplementedException();

			foreach (DwgTemplate template in this.Templates.Values)
			{
				template.Build(this);
			}
		}

		public CadObject GetCadObject(ulong handle)
		{
			if (this.ObjectsMap.TryGetValue(handle, out CadObject cadObject))
				return cadObject;
			else
				return null;
		}

		public T GetCadObject<T>(ulong handle) where T : CadObject
		{
			if (this.ObjectsMap.TryGetValue(handle, out CadObject cadObject))
			{
				if (cadObject is T)
					return (T)cadObject;
			}

			return null;
		}

		public bool TryGetCadObject<T>(ulong handle, out T value) where T : CadObject
		{
			if (this.ObjectsMap.TryGetValue(handle, out CadObject cadObject))
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

		public T GetObjectTemplate<T>(ulong handle) where T : DwgTemplate
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate builder))
			{
				return (T)builder;
			}

			return null;
		}

		public bool TryGetObjectTemplate<T>(ulong handle, out T value) where T : DwgTemplate
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate template))
			{
				if (template is T)
				{
					value = (T)template;
					return true;
				}
			}

			value = null;
			return false;
		}
	}
}
