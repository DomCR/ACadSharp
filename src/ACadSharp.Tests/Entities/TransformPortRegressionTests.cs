using ACadSharp.Entities;
using ACadSharp.Entities.AecEntities;
using ACadSharp.Objects;
using CSMath;
using System;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class TransformPortRegressionTests
{
	[Fact]
	public void MultiLeaderTransformsAnchorsDirectionsBreaksAndSizes()
	{
		var entity = new MultiLeader { ArrowheadSize = 2, LandingDistance = 3 };
		entity.ContextData.TextLocation = new XYZ(1, 0, 0);
		entity.ContextData.TextNormal = XYZ.AxisZ;
		entity.ContextData.TextHeight = 4;
		var root = new MultiLeaderObjectContextData.LeaderRoot { ConnectionPoint = new XYZ(1, 0, 0), Direction = XYZ.AxisX };
		var line = new MultiLeaderObjectContextData.LeaderLine { ArrowheadSize = 5 };
		line.Points.Add(new XYZ(2, 0, 0));
		var info = new MultiLeaderObjectContextData.BreakInfo();
		info.StartEndPoints.Add(new MultiLeaderObjectContextData.StartEndPointPair(XYZ.Zero, XYZ.AxisX));
		line.BreakInfoEntries.Add(info);
		root.Lines.Add(line);
		entity.ContextData.LeaderRoots.Add(root);
		var transform = new Transform(new XYZ(10, 20, 30), new XYZ(2, 2, 2), new XYZ(0, 0, Math.PI / 2));
		entity.ApplyTransform(transform);
		Assert.Equal(transform.ApplyTransform(new XYZ(1, 0, 0)), entity.ContextData.TextLocation);
		Assert.Equal(transform.ApplyTransform(new XYZ(2, 0, 0)), line.Points[0]);
		Assert.Equal(transform.ApplyTransform(XYZ.Zero), info.StartEndPoints[0].StartPoint);
		Assert.Equal(transform.ApplyTransform(XYZ.AxisX), info.StartEndPoints[0].EndPoint);
		Assert.Equal(1, root.Direction.Y, 8);
		Assert.Equal(0, root.Direction.X, 8);
		Assert.Equal(XYZ.AxisZ, entity.ContextData.TextNormal);
		Assert.Equal(8, entity.ContextData.TextHeight);
		Assert.Equal(4, entity.ArrowheadSize);
		Assert.Equal(10, line.ArrowheadSize);
		Assert.Equal(Math.PI / 2, entity.ContextData.TextRotation);
	}

	[Fact]
	public void WallBoundsAndDimensionsFollowNonUniformScale()
	{
		var wall = new Wall { StartPoint = XYZ.Zero, EndPoint = new XYZ(10, 0, 0), Width = 2, Height = 3,
			BaseHeight = 1, Justification = WallJustification.Center };
		Assert.Equal(new XYZ(0, -1, 1), wall.GetBoundingBox().Min);
		Assert.Equal(new XYZ(10, 1, 4), wall.GetBoundingBox().Max);
		wall.ApplyTransform(new Transform(new XYZ(5, 6, 7), new XYZ(2, 3, 4), XYZ.Zero));
		Assert.Equal(20, wall.Length);
		Assert.Equal(6, wall.Width);
		Assert.Equal(12, wall.Height);
		Assert.Equal(4, wall.BaseHeight);
		Assert.Equal(new XYZ(5, 3, 11), wall.GetBoundingBox().Min);
		Assert.Equal(new XYZ(25, 9, 23), wall.GetBoundingBox().Max);
	}
}
