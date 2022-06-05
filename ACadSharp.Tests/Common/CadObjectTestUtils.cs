using ACadSharp.Entities;
using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests.Common
{
	public class CadObjectTestUtils
	{
		public static void AssertClone(CadObject original, CadObject clone, bool hasOwner = false)
		{
			//Assert clone
			Assert.NotEqual(original, clone);
			Assert.True(0 == clone.Handle);
			Assert.Null(clone.Document);

			if (!hasOwner)
				Assert.Null(clone.Owner);
		}

		public static void AssertEntityClone(Entity original, Entity clone, bool hasOwner = false)
		{
			AssertClone(original, clone, hasOwner);

			Assert.Equal(original.LinetypeScale, clone.LinetypeScale);

			//Assert clone
			AssertTableEntryClone(original.Layer, clone.Layer);
			AssertTableEntryClone(original.LineType, clone.LineType);
		}

		public static void AssertTableEntryClone(TableEntry original, TableEntry clone, bool hasOwner = false)
		{
			AssertClone(original, clone, hasOwner);

			//Assert clone
			Assert.True(clone.Name == original.Name);
			Assert.True(clone.Flags == original.Flags);
		}
	}
}
