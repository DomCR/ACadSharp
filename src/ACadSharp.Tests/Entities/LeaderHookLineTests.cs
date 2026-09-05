using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class LeaderHookLineTests
{
	[Fact]
	public void AHookAgainstTheHorizontalDirectionIsAHook()
	{
		//A leader whose text sits to its right ends with a hook that runs from the last vertex
		//back toward the leader, against +X. That is HookLineDirection.Opposite, and it is a hook.
		Leader leader = new Leader();
		leader.Vertices.Add(new XYZ(0, 0, 0));
		leader.Vertices.Add(new XYZ(10, 5, 0));
		leader.Vertices.Add(new XYZ(12.5, 5, 0));

		Assert.True(leader.HasHookline);
	}

	[Fact]
	public void AHookAlongTheHorizontalDirectionIsAHook()
	{
		Leader leader = new Leader();
		leader.Vertices.Add(new XYZ(20, 0, 0));
		leader.Vertices.Add(new XYZ(10, 5, 0));
		leader.Vertices.Add(new XYZ(7.5, 5, 0));

		Assert.True(leader.HasHookline);
	}

	[Fact]
	public void ASlopedLastSegmentIsNotAHook()
	{
		Leader leader = new Leader();
		leader.Vertices.Add(new XYZ(0, 0, 0));
		leader.Vertices.Add(new XYZ(10, 5, 0));
		leader.Vertices.Add(new XYZ(12, 7, 0));

		Assert.False(leader.HasHookline);
	}

	[Fact]
	public void TheLeaderAutoCadWroteWithAHookIsReadWithOne()
	{
		//samples/sample_AC1032_ascii.dxf carries one LEADER, handle 512, that AutoCAD saved with
		//75 = 1 and a last segment of 2.5 units running -X toward its MTEXT. It used to be read as
		//having no hook line.
		string path = Path.Combine(TestVariables.SamplesFolder, "sample_AC1032_ascii.dxf");
		CadDocument doc = DxfReader.Read(path);

		Leader leader = Assert.Single(doc.Entities.OfType<Leader>());
		Assert.Equal(0x512UL, leader.Handle);
		Assert.True(leader.HasHookline);
		Assert.Equal(HookLineDirection.Opposite, leader.HookLineDirection);
	}
}
