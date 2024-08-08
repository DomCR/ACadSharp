#if !NET7_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace CSUtilities
{
	/// <summary>
	/// Class that allows to modify, copy or control objects outside their own methods.
	/// </summary>
	internal static class Mutation
	{
		/// <summary>
		/// Clone the serializable object into a new instance.
		/// </summary>
		/// <typeparam name="T">Type of the serializable object.</typeparam>
		/// <param name="source">Object to clone.</param>
		/// <returns>The new instance of the object.</returns>
		public static T CloneSerializable<T>(T source)
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", "source");
			}

			if (Object.ReferenceEquals(source, null))
			{
				return default(T);
			}

			System.Runtime.Serialization.IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}

		/// <summary>
		/// Deep copy of an object using reflection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns>A copy of the object</returns>
		public static T DeepClone<T>(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("Object cannot be null");
			return (T)processObject(obj);
		}

		/// <summary>
		/// store the info of an object in a dictionary
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns>Returns a dictionary with [paramName, paramValue]</returns>
		public static Dictionary<string, object> ExplodeObject<T>(T instance)
		{
			Type tp = typeof(T);

			Dictionary<string, object> properties = new Dictionary<string, object>();

			FieldInfo[] fields = tp.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

			//FieldInfo[] fields = tp.GetFields();
			MethodInfo[] mts = tp.GetMethods();

			foreach (MethodInfo mt in mts)
			{
				if (mt.GetMethodBody() != null)
					properties.Add(mt.Name, mt.GetMethodBody());
			}

			foreach (FieldInfo info in fields)
			{
				var some = info.GetValue(instance);
				var s = info.Name;

				if (info.IsPrivate)
					properties.Add(info.Name, info.GetValue(instance));
			}

			foreach (PropertyInfo prop in tp.GetProperties())
				properties.Add(prop.Name, prop.GetValue(instance));

			return properties;
		}

		/// <summary>
		/// Serialize an object into a file.
		/// </summary>
		/// <param name="path">File path.</param>
		/// <param name="obj">Object to serialize.</param>
		public static void Serialize(string path, object obj)
		{
			FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			BinaryFormatter formater = new BinaryFormatter();
			formater.Serialize(stream, obj);
			stream.Close();
		}

		/// <summary>
		/// Deserialize an object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns>The deserializated object.</returns>
		public static T Deserialize<T>(string path)
		{
			//FileStream stream = new FileStream(path, FileMode.Open);
			//BinaryFormatter formater = new BinaryFormatter();
			//formater.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
			//object result = formater.Deserialize(stream);
			//stream.Close();
			//return (T)result;

			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserialize an object.
		/// </summary>
		/// <param name="path"></param>
		/// <returns>The deserializated object.</returns>
		public static object Deserialize(string path)
		{
			return Deserialize<object>(path);
		}
		
		private static object processObject(object obj)
		{
			if (obj == null)
				return null;
			//Get the type and if it has a copiable value
			Type tp = obj.GetType();
			if (tp.IsValueType || tp == typeof(string))
			{
				return obj;
			}
			else if (tp.IsArray)
			{
				//Process every element of the array
				Type elementType = Type.GetType(tp.FullName.Replace("[]", string.Empty));
				Array array = obj as Array;
				Array copied = Array.CreateInstance(elementType, array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					//Recursive: repeat the process foreach value of the array
					copied.SetValue(processObject(array.GetValue(i)), i);
				}
				//Return the array as the object
				return Convert.ChangeType(copied, obj.GetType());
			}
			else if (tp.IsClass)
			{
				//Process class
				object copy = Activator.CreateInstance(obj.GetType());
				FieldInfo[] fields = tp.GetFields(BindingFlags.Public |
							BindingFlags.NonPublic | BindingFlags.Instance);
				foreach (FieldInfo field in fields)
				{
					object fieldValue = field.GetValue(obj);
					if (fieldValue == null)
						continue;
					//Recursive: foreach field repeat the process to copy the value
					field.SetValue(copy, processObject(fieldValue));
				}
				//Return the completed copy of the object
				return copy;
			}
			//TODO: Handle the other object possibilities
			else
				throw new ArgumentException("Unknown type");
		}
	}
}
#endif