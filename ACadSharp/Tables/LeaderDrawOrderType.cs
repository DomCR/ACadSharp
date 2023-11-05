namespace ACadSharp.Tables {

	/// <summary>
	/// Specifies the draw order of a <see cref="Objects.MultiLeaderAnnotContext.LeaderRoot"/>
	/// in a <see cref="Entities.MultiLeader"/> entity.
	/// </summary>
	public enum LeaderDrawOrderType {

		/// <summary>
		/// 0 = draw leader head first
		/// </summary>
		LeaderHeadFirst = 0,

		/// <summary>
		/// 1 = draw leader tail first
		/// </summary>
		LeaderTailFirst = 1,
	}
}