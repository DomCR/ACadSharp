namespace ACadSharp.Objects
{
	[System.Flags]
	public enum AssociativityFlags : short
	{
		None = 0,

		FirstPointReference = 1,

		SecondPointReference = 2,

		ThirdPointReference = 4,

		FourthPointReference = 8
	}
}