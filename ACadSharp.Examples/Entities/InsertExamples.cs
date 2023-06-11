using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp.Examples.Entities
{
	public static class InsertExamples
	{
		/// <summary>
		/// Create an insert referencing a block
		/// </summary>
		public static void CreateInsert()
		{
			//Create a block record to use as a reference
			BlockRecord record = new BlockRecord("my_block");
			record.Entities.Add(new Line());
			record.Entities.Add(new Point());

			//Create an insert referencing the block record
			Insert insert = new Insert(record);
		}

		/// <summary>
		/// Create an insert entity and add it to the model
		/// </summary>
		public static void AddInsertIntoModel()
		{
			//Create a block record to use as a reference
			BlockRecord record = new BlockRecord("my_block");
			record.Entities.Add(new Point());

			//Create an insert referencing the block record
			Insert insert = new Insert(record);
			//Set the insert point and scale fields
			insert.InsertPoint = new CSMath.XYZ(100, 100, 0);
			insert.XScale = 2;
			insert.YScale = 2;

			//Add the insert into a document
			CadDocument doc = new CadDocument();

			//Add the insert into the model
			doc.Entities.Add(insert);

			//Once the file is saved you will have an instance of my_block scaled by 2 and at the point 100, 100
		}
	}
}
