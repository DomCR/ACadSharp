using ACadSharp.IO;
using System.Collections.Generic;

namespace ACadSharp.Examples
{
	/// <summary>
	/// Examples of conversion and saving documents
	/// </summary>
	public class IOExamples
	{
		/// <summary>
		/// Read a dwg and save the entities in a new file
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		public void DwgEntitiesToNewFile(string input, string output)
		{
			/* --- ATENTION --- 
			 * ACadSharp cannot write a readed dwg/dxf file due a problem in the file structure when the dxf is writen
			 * the workaround for now is to move the entities and save them in a new file
			 */
			CadDocument doc = DwgReader.Read(input);

			//New document to transfer the entities
			CadDocument transfer = new CadDocument();
			doc.Header.Version = doc.Header.Version;

			//Nove the entities to the created document
			List<ACadSharp.Entities.Entity> entities = new List<ACadSharp.Entities.Entity>(doc.Entities);
			foreach (var item in entities)
			{
				ACadSharp.Entities.Entity e = doc.Entities.Remove(item);
				transfer.Entities.Add(e);
			}

			//Save the document
			using (DxfWriter writer = new DxfWriter(output, doc, false))
			{
				writer.OnNotification += Common.NotificationHelper.LogConsoleNotification;
				writer.Write();
			}
		}
	}
}
