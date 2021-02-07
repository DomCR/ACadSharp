using ACadSharp.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACadSharp
{
	public abstract class CadObject
	{
		/// <summary>
		/// The AutoCAD class name of an object.
		/// </summary>
		public virtual string ObjectName { get; } = DxfFileToken.Undefined;
		/// <summary>
		/// The document (drawing) that contains the object.
		/// </summary>
		public CadDocument Document { get; }
		/// <summary>
		/// The handle of the entity.
		/// </summary>
		[DxfCodeValue(DxfCode.Handle)]
		public ulong Handle { get; set; }
		/// <summary>
		/// Soft-pointer ID/handle to owner object
		/// </summary>
		[DxfCodeValue(DxfCode.SoftPointerId)]
		public ulong OwnerHandle { get; set; }

		/// <summary>
		/// Objects objects that has been attached to this entity.
		/// </summary>
		public Dictionary<ulong, CadObject> Reactors { get; set; } = new Dictionary<ulong, CadObject>();

		//TODO: Extended data

		/// <summary>
		/// Get a map of the object using the dxf codes in each field.
		/// </summary>
		/// <returns></returns>
		internal Dictionary<DxfCode, object> GetCadObjectMap()
		{
			Dictionary<DxfCode, object> map = new Dictionary<DxfCode, object>();

			foreach (PropertyInfo p in GetType().GetProperties())
			{
				DxfCodeValueAttribute att = p.GetCustomAttribute<DxfCodeValueAttribute>();
				if (att == null)
					continue;

				//Set the codes to the map
				foreach (DxfCode code in att.ValueCodes)
				{
					map.Add(code, null);
				}
			}

			return map;
		}
		/// <summary>
		/// Build the entity using a map with the dxf codes and the values.
		/// </summary>
		/// <param name="map"></param>
		internal virtual void Build(Dictionary<DxfCode, object> map)
		{
			var a = GetType().GetProperties();

			foreach (PropertyInfo p in GetType().GetProperties())
			{
				DxfCodeValueAttribute att = p.GetCustomAttribute<DxfCodeValueAttribute>();
				if (att == null)
					continue;

				//Get the parameters or value to build the property
				List<object> parameters = new List<object>();
				foreach (DxfCode code in att.ValueCodes)
				{
					if (map.TryGetValue(code, out object par))
					{
						parameters.Add(par);
					}
				}

				//Check for invalid values
				while (parameters.Contains(null))
					parameters.Remove(null);

				if (!parameters.Any())
					continue;

				//Create an object with the same type of the property type
				object value = null;
				//Check if has a constructor with parameters
				ConstructorInfo constr = p.PropertyType.GetConstructor(parameters.Select(o => o.GetType()).ToArray());

				//Fill the value
				if (p.PropertyType.IsEnum)
				{
					value = Enum.ToObject(p.PropertyType, parameters.First());
				}
				else if (constr == null)
				{
					value = Convert.ChangeType(parameters.First(), p.PropertyType);
				}
				else
				{
					value = constr.Invoke(parameters.ToArray());
				}

				//Set the value if it has any
				if (value != null)
				{
					p.SetValue(this, value);
				}
			}
		}

		public override string ToString()
		{
			return ObjectName;
		}
	}
}
