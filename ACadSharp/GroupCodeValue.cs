namespace ACadSharp
{
	public static class GroupCodeValue
	{
		public static GroupCodeValueType TransformValue(int code)
		{
			if (code >= 0 && code <= 4)
				return GroupCodeValueType.String;
			if (code == 5)
				return GroupCodeValueType.Handle;
			if (code >= 6 && code <= 9)
				return GroupCodeValueType.String;
			if (code >= 10 && code <= 39)
				return GroupCodeValueType.Point3D;
			if (code >= 40 && code <= 59)
				return GroupCodeValueType.Double;
			if (code >= 60 && code <= 79)
				return GroupCodeValueType.Int16;
			if (code >= 90 && code <= 99)
				return GroupCodeValueType.Int32;
			if (code == 100)
				return GroupCodeValueType.String;
			if (code == 101)
				return GroupCodeValueType.String;
			if (code == 102)
				return GroupCodeValueType.String;
			if (code == 105)
				return GroupCodeValueType.Handle;

			if (code >= 110 && code <= 119)
				return GroupCodeValueType.Double;
			if (code >= 120 && code <= 129)
				return GroupCodeValueType.Double;
			if (code >= 130 && code <= 139)
				return GroupCodeValueType.Double;
			if (code >= 140 && code <= 149)
				return GroupCodeValueType.Double;

			if (code >= 160 && code <= 169)
				return GroupCodeValueType.Int64;

			if (code >= 170 && code <= 179)
				return GroupCodeValueType.Int16;

			if (code >= 210 && code <= 239)
				return GroupCodeValueType.Double;

			if (code >= 270 && code <= 279)
				return GroupCodeValueType.Int16;

			if (code >= 280 && code <= 289)
				return GroupCodeValueType.Byte;

			if (code >= 290 && code <= 299)
				return GroupCodeValueType.Bool;

			if (code >= 300 && code <= 309)
				return GroupCodeValueType.String;

			if (code >= 310 && code <= 319)
				return GroupCodeValueType.Chunk;

			if (code >= 320 && code <= 329)
				return GroupCodeValueType.Handle;

			if (code >= 330 && code <= 369)
				return GroupCodeValueType.ObjectId;

			if (code >= 370 && code <= 379)
				return GroupCodeValueType.Int16;
			if (code >= 380 && code <= 389)
				return GroupCodeValueType.Int16;

			if (code >= 390 && code <= 399)
				return GroupCodeValueType.Handle;

			if (code >= 400 && code <= 409)
				return GroupCodeValueType.Int16;
			if (code >= 410 && code <= 419)
				return GroupCodeValueType.String;
			if (code >= 420 && code <= 429)
				return GroupCodeValueType.Int32;
			if (code >= 430 && code <= 439)
				return GroupCodeValueType.String;
			if (code >= 440 && code <= 449)
				return GroupCodeValueType.Int32;
			if (code >= 450 && code <= 459)
				return GroupCodeValueType.Int32;
			if (code >= 460 && code <= 469)
				return GroupCodeValueType.Double;
			if (code >= 470 && code <= 479)
				return GroupCodeValueType.String;
			if (code >= 480 && code <= 481)
				return GroupCodeValueType.Handle;

			if (code == 999)
				return GroupCodeValueType.Comment;

			if (code >= 1000 && code <= 1003)
				return GroupCodeValueType.ExtendedDataString;
			if (code == 1004)
				return GroupCodeValueType.ExtendedDataChunk;
			if (code >= 1005 && code <= 1009)
				return GroupCodeValueType.ExtendedDataHandle;
			if (code >= 1010 && code <= 1059)
				return GroupCodeValueType.ExtendedDataDouble;
			if (code >= 1060 && code <= 1070)
				return GroupCodeValueType.ExtendedDataInt16;
			if (code == 1071)
				return GroupCodeValueType.ExtendedDataInt32;

			return GroupCodeValueType.None;
		}
	}
}