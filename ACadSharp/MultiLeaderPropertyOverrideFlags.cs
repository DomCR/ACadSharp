using System;

namespace ACadSharp {

	[Flags]
	public enum MultiLeaderPropertyOverrideFlags : int {

		/// <summary>
		/// No Flag: No property to be overridden
		/// </summary>
		None = 0,

		/// <summary>
		/// 
		/// </summary>
		PathType = 0x1,

		/// <summary>
		/// LineColor property / 1 << 1
		/// </summary>
		LineColor = 0x2,

		/// <summary>
		/// LeaderLineType property	/1 << 2
		/// </summary>
		LeaderLineType = 0x4,

		/// <summary>
		/// LeaderLineWeight property / 1 << 3
		/// </summary>
		LeaderLineWeight = 0x8,

		/// <summary>
		/// EnableLanding property / 1 << 4
		/// </summary>
		EnableLanding = 0x10,

		/// <summary>
		/// / 1 << 5
		/// </summary>
		LandingGap = 0x20,

		/// <summary>
		/// EnableDogleg property / 1 << 6 = Enable dog-leg,
		/// </summary>
		EnableDogleg = 0x40,

		/// <summary>
		/// LandingDistance property / 1 << 7 = Dog-leg length,
		/// </summary>
		LandingDistance = 0x80,

		/// <summary>
		/// Arrowhead property / 1 << 8 = Arrow symbol handle,
		/// </summary>
		Arrowhead = 0x100,

		/// <summary>
		/// ArrowheadSize property / 1 << 9 = Arrow size,
		/// </summary>
		ArrowheadSize = 0x200,

		/// <summary>
		/// ContentType property / 1 << 10 = Content type
		/// </summary>
		ContentType = 0x400,

		/// <summary>
		/// TextStyle property / 1 << 11 = Text style handle,
		/// </summary>
		TextStyle = 0x800,

		/// <summary>
		/// TextLeftAttachment property / 1 << 12 = Text left attachment type (of MTEXT),
		/// </summary>
		TextLeftAttachment = 0x1000,

		/// <summary>
		/// TextAngle property / 1 << 13 = Text angle type (of MTEXT),
		/// </summary>
		TextAngle = 0x2000,

		/// <summary>
		/// TextAlignment property / 1 << 14 = Text alignment type (of MTEXT),
		/// </summary>
		TextAlignment = 0x4000,

		/// <summary>
		/// TextColor property / 1 << 15 = Text color (of MTEXT),
		/// </summary>
		TextColor = 0x8000,

		//1 << 16 = Text height (of MTEXT),
		TextHeight = 0x10000,

		/// <summary>
		/// TextFrame property / 1 << 17 = Enable text frame,
		/// </summary>
		TextFrame = 0x20000,

		//1 << 18 = Enable use of default MTEXT (from MLEADERSTYLE),
		EnableUseDefaultMText = 0x40000,

		/// <summary>
		/// BlockContent property / 1 << 19 = Content block handle.
		/// </summary>
		BlockContent = 0x80000,

		/// <summary>
		/// BlockContentColor property / 1 << 20 = Block content color.
		/// </summary>
		//1 << 20 = Block content color,
		BlockContentColor = 0x100000,

		/// <summary>
		/// BlockContentScale / 1 << 21 = Block content scale.
		/// </summary>
		BlockContentScale = 0x200000,

		/// <summary>
		/// BlockContentRotation property / 1 << 22 = Block content rotation.
		/// </summary>
		
		BlockContentRotation = 0x400000,

		/// <summary>
		/// BlockContentConnection property / 1 << 23 = Block connection type,
		/// </summary>
		BlockConnectionConnection = 0800000,

		
		/// <summary>
		/// ScaleFactor property / 1 << 24 = Scale,
		/// </summary>
		ScaleFactor = 0x1000000,

		/// <summary>
		///	TextRightAttachment property / 1 << 25 = Text right attachment type (of MTEXT),
		/// </summary>
		TextRightAttachment = 0x2000000,

		/// <summary>
		///	?? which property / 1 << 26 = Text switch alignment type (of MTEXT),
		/// </summary>
		TextSwitchAlignmentType = 0x4000000,

		/// <summary>
		/// TextAttachmentDirection property / 1 << 27 = Text attachment direction (of MTEXT),
		/// </summary>
		TextAttachmentDirection = 0x8000000,

		///<summary>
		/// TextTopAttachment property / 1 << 28 = Text top attachment type (of MTEXT),
		/// </summary>
		TextTopAttachment= 0x10000000,

		/// <summary>
		/// TextBottomAttachment property = Text bottom attachment type of MTEXT (1 << 29)
		/// </summary>
		TextBottomAttachment = 0x20000000
	}
}
