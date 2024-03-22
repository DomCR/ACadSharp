//	TODO similar to LeaderPathType
//  0 = straight line leader, 1 = spline leader


namespace ACadSharp {

	/// <summary>
	/// Controls the way the leader is drawn.
	/// </summary>
	public  enum MultiLeaderPathType {

		/// <summary>
		/// Invisible leader
		/// </summary>
		Invisible = 0,

		/// <summary>
		/// Draws the leader line as a set of straight line segments
		/// </summary>
		StraightLineSegments = 1,

		/// <summary>
		/// Draws the leader line as a spline
		/// </summary>
		Spline = 2
	}
}
