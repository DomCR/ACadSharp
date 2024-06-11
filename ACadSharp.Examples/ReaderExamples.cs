using ACadSharp.Entities;
using ACadSharp.Examples.Common;
using ACadSharp.IO;
using System.Collections.Generic;

namespace ACadSharp.Examples
{
	public static class ReaderExamples
	{
		/// <summary>
		/// Read a dxf file
		/// </summary>
		/// <param name="file">dxf file path</param>
		public static void ReadDxf(string file)
		{
			using (DxfReader reader = new DxfReader(file))
			{
				reader.OnNotification += NotificationHelper.LogConsoleNotification;
				CadDocument doc = reader.Read();
			}
		}

		/// <summary>
		/// Read a dwg file
		/// </summary>
		/// <param name="file">dwg file path</param>
		public static void ReadDwg(string file)
		{
			using (DwgReader reader = new DwgReader(file))
			{
				reader.OnNotification += NotificationHelper.LogConsoleNotification;
				CadDocument doc = reader.Read();
			}
		}

		/// <summary>
		/// Read the entities section from a dxf file and add them to a <see cref="CadDocument"/>
		/// </summary>
		/// <param name="file"></param>
		public static void ReadEntitiesFromDxf(string file)
		{
			CadDocument doc = new CadDocument();
			List<Entity> entities = new List<Entity>();

			using (DxfReader reader = new DxfReader(file))
			{
				reader.OnNotification += NotificationHelper.LogConsoleNotification;
				entities = reader.ReadEntities();
			}

			doc.Entities.AddRange(entities);
		}

		/// <summary>
		/// Read the tables section from a dxf file.
		/// </summary>
		/// <param name="file"></param>
		public static void ReadTablesFromDxf(string file)
		{
			using (DxfReader reader = new DxfReader(file))
			{
				reader.OnNotification += NotificationHelper.LogConsoleNotification;
				CadDocument doc = reader.ReadTables();
			}
		}
	}
}
