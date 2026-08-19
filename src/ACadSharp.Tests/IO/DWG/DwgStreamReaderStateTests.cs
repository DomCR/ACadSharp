using ACadSharp.IO.DWG;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class DwgStreamReaderStateTests
{
	/// <summary>
	/// Moving a reader to another object has to decide again whether that object has a string
	/// stream. The flag used to be latched on, so a reader that had seen one object without a
	/// string stream reported every later object as empty too, and the values read after that were
	/// wrong without anything being thrown.
	/// </summary>
	[Fact]
	public void MovingToAnObjectWithAStringStreamClearsTheEmptyFlag()
	{
		//A zeroed buffer with a single bit set: at bit 100 the string stream flag is 1, and the
		//size field it then reads, at bit 84, is zero, which is a string stream of length zero.
		byte[] buffer = new byte[32];
		buffer[12] = 0x08;

		IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(
			ACadVersion.AC1032,
			new MemoryStream(buffer));

		//Bit 200 is zero: no string stream.
		reader.SetPositionByFlag(200);
		Assert.True(reader.IsEmpty, "an object without a string stream should report empty");

		//The same reader, moved to the object whose flag is set.
		reader.SetPositionByFlag(100);
		Assert.False(reader.IsEmpty, "a reader moved to an object with a string stream is not empty");
	}

	[Fact]
	public void AFreshReaderAndAMovedOneAgreeOnTheSameObject()
	{
		byte[] buffer = new byte[32];
		buffer[12] = 0x08;

		IDwgStreamReader fresh = DwgStreamReaderBase.GetStreamHandler(
			ACadVersion.AC1032,
			new MemoryStream(buffer));
		long freshPosition = fresh.SetPositionByFlag(100);

		IDwgStreamReader moved = DwgStreamReaderBase.GetStreamHandler(
			ACadVersion.AC1032,
			new MemoryStream(buffer));
		moved.SetPositionByFlag(200);
		long movedPosition = moved.SetPositionByFlag(100);

		Assert.Equal(fresh.IsEmpty, moved.IsEmpty);
		Assert.Equal(freshPosition, movedPosition);
		Assert.Equal(fresh.PositionInBits(), moved.PositionInBits());
	}
}
