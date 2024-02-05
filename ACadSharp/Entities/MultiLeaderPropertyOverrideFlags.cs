using System;
using ACadSharp.Objects;

namespace ACadSharp.Entities
{
	[Flags]
	public enum MultiLeaderPropertyOverrideFlags : int
	{
		/// <summary>
		/// No Flag: No property to be overridden
		/// </summary>
		None = 0,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.PathType"/> property.
		/// </summary>
		PathType = 0x1,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.LineColor" /> property.
		/// </summary>
		LineColor = 0x2,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.LeaderLineType" /> property.
		/// </summary>
		LeaderLineType = 0x4,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.LeaderLineWeight" /> property.
		/// </summary>
		LeaderLineWeight = 0x8,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.EnableLanding" /> property.
		/// </summary>
		EnableLanding = 0x10,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.LandingGap"/> property.
		/// </summary>
		LandingGap = 0x20,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.EnableDogleg"/> property.
		/// </summary>
		EnableDogleg = 0x40,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.LandingDistance"/> property.
		/// </summary>
		LandingDistance = 0x80,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.Arrowhead"/> property.
		/// </summary>
		Arrowhead = 0x100,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.ArrowheadSize"/> property.
		/// </summary>
		ArrowheadSize = 0x200,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.ContentType"/> property.
		/// </summary>
		ContentType = 0x400,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextStyle"/> property.
		/// </summary>
		TextStyle = 0x800,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextLeftAttachment"/> property.
		/// </summary>
		TextLeftAttachment = 0x1000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextAngle"/> property.
		/// </summary>
		TextAngle = 0x2000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextAlignment"/> property.
		/// </summary>
		TextAlignment = 0x4000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextColor"/> property.
		/// </summary>
		TextColor = 0x8000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextHeight" /> property.
		/// </summary>
		TextHeight = 0x10000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextFrame"/> property.
		/// </summary>
		TextFrame = 0x20000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.??" /> property.
		/// </summary>
		EnableUseDefaultMText = 0x40000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.BlockContent"/> property.
		/// </summary>
		BlockContent = 0x80000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.BlockContentColor"/> property.
		/// </summary>
		BlockContentColor = 0x100000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.BlockContentScale"/> property.
		/// </summary>
		BlockContentScale = 0x200000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.BlockContentRotation"/> property.
		/// </summary>
		BlockContentRotation = 0x400000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.BlockContentConnection"/> property.
		/// </summary>
		BlockConnectionConnection = 0x800000,


		/// <summary>
		/// Override <see cref="MultiLeaderStyle.ScaleFactor"/> property.
		/// </summary>
		ScaleFactor = 0x1000000,

		/// <summary>
		///	Override <see cref="MultiLeaderStyle.TextRightAttachment"/> property.
		/// </summary>
		TextRightAttachment = 0x2000000,

		/// <summary>
		///	Override <see cref="MultiLeaderStyle.?? which"/> property.
		/// </summary>
		TextSwitchAlignmentType = 0x4000000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextAttachmentDirection"/> property.
		/// </summary>
		TextAttachmentDirection = 0x8000000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextTopAttachment"/> property.
		/// </summary>
		TextTopAttachment = 0x10000000,

		/// <summary>
		/// Override <see cref="MultiLeaderStyle.TextBottomAttachment"/> property.
		/// </summary>
		TextBottomAttachment = 0x20000000
	}
}
