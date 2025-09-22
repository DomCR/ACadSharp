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
			Assert.False(original.Equals(clone));
			Assert.True(0 == clone.Handle);
			Assert.Null(clone.Document);
			Assert.Empty(clone.ExtendedData);
		}

		public static void AssertEntityClone(Entity original, Entity clone)
		{
			AssertClone(original, clone);

			Assert.Equal(original.LineTypeScale, clone.LineTypeScale);

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

		public static void AssertEntityCollection<T>(CadObjectCollection<T> original, CadObjectCollection<T> clone)
			where T : Entity
		{
			Assert.NotEmpty(original);
			for (int i = 0; i < original.Count; i++)
			{
				CadObjectTestUtils.AssertEntityClone(original[i], clone[i]);
			}
		}
	}
}
