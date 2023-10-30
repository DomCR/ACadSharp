using ACadSharp.Entities;
using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests.Common
{
	public class CadObjectTestUtils
	{
		public static void AssertClone(CadObject original, CadObject clone)
		{
			//Assert clone
			Assert.NotEqual(original, clone);
			Assert.True(0 == clone.Handle);
			Assert.Null(clone.Document);
		}

		public static void AssertEntityClone(Entity original, Entity clone)
		{
			AssertClone(original, clone);

			Assert.Equal(original.LinetypeScale, clone.LinetypeScale);

			//Assert clone
			AssertTableEntryClone(original.Layer, clone.Layer);
			AssertTableEntryClone(original.LineType, clone.LineType);
		}

		public static void AssertTableEntryClone(TableEntry original, TableEntry clone)
		{
			AssertClone(original, clone);

			//Assert clone
			Assert.True(clone.Name == original.Name);
			Assert.True(clone.Flags == original.Flags);
		}
	}
}
