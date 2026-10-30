namespace ACadSharp.IO.DWG;

internal abstract class DwgLZ77AC21
{
	private delegate void copyDelegate(byte[] src, uint srcIndex, byte[] dst, uint dstIndex);

	/// <summary>
	/// The copying happens in chunks of 32 bytes,
	/// and the remainder is copied using a specific copy function for each number of bytes
	/// (so 31 separate copy functions). For copying 1-32 bytes, a combination of sub byte blocks is made:
	/// </summary>
	private static readonly copyDelegate[] _copyMethods = new copyDelegate[32]
{
	null,
	(src, srcIndex, dst, dstIndex) =>{copy1b(src, srcIndex, dst, dstIndex); },
	(src, srcIndex, dst, dstIndex) =>{copy2b(src, srcIndex, dst, dstIndex); },
	(src, srcIndex, dst, dstIndex) =>{copy3b(src, srcIndex, dst, dstIndex); },
	(src, srcIndex, dst, dstIndex) =>{copy4b(src, srcIndex, dst, dstIndex); },
	(src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 4U, dst, dstIndex);
	  copy4b(src, srcIndex, dst, dstIndex + 1U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 5U, dst, dstIndex);
	  copy4b(src, srcIndex + 1U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex, dst, dstIndex + 5U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 5U, dst, dstIndex);
	  copy4b(src, srcIndex + 1U, dst, dstIndex + 2U);
	  copy1b(src, srcIndex, dst, dstIndex + 6U);
	},
	new copyDelegate(copy8b),
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 8U, dst, dstIndex);
	  copy8b(src, srcIndex, dst, dstIndex + 1U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 9U, dst, dstIndex);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex, dst, dstIndex + 9U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 9U, dst, dstIndex);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 2U);
	  copy1b(src, srcIndex, dst, dstIndex + 10U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy4b(src, srcIndex + 8U, dst, dstIndex);
	  copy8b(src, srcIndex, dst, dstIndex + 4U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 12U, dst, dstIndex);
	  copy4b(src, srcIndex + 8U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex, dst, dstIndex + 5U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 13U, dst, dstIndex);
	  copy4b(src, srcIndex + 9U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 5U);
	  copy1b(src, srcIndex, dst, dstIndex + 13U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 13U, dst, dstIndex);
	  copy4b(src, srcIndex + 9U, dst, dstIndex + 2U);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 6U);
	  copy1b(src, srcIndex, dst, dstIndex + 14U);
	},
	new copyDelegate(copy16b),
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy8b(src, srcIndex + 9U, dst, dstIndex);
	  copy1b(src, srcIndex + 8U, dst, dstIndex + 8U);
	  copy8b(src, srcIndex, dst, dstIndex + 9U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 17U, dst, dstIndex);
	  copy16b(src, srcIndex + 1U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex, dst, dstIndex + 17U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy3b(src, srcIndex + 16U, dst, dstIndex);
	  copy16b(src, srcIndex, dst, dstIndex + 3U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy4b(src, srcIndex + 16U, dst, dstIndex);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 4U);
	  copy8b(src, srcIndex, dst, dstIndex + 12U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 20U, dst, dstIndex);
	  copy4b(src, srcIndex + 16U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 5U);
	  copy8b(src, srcIndex, dst, dstIndex + 13U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 20U, dst, dstIndex);
	  copy4b(src, srcIndex + 16U, dst, dstIndex + 2U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 6U);
	  copy8b(src, srcIndex, dst, dstIndex + 14U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy3b(src, srcIndex + 20U, dst, dstIndex);
	  copy4b(src, srcIndex + 16U, dst, dstIndex + 3U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 7U);
	  copy8b(src, srcIndex, dst, dstIndex + 15U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy8b(src, srcIndex + 16U, dst, dstIndex);
	  copy16b(src, srcIndex, dst, dstIndex + 8U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy8b(src, srcIndex + 17U, dst, dstIndex);
	  copy1b(src, srcIndex + 16U, dst, dstIndex + 8U);
	  copy16b(src, srcIndex, dst, dstIndex + 9U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 25U, dst, dstIndex);
	  copy8b(src, srcIndex + 17U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex + 16U, dst, dstIndex + 9U);
	  copy16b(src, srcIndex, dst, dstIndex + 10U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 25U, dst, dstIndex);
	  copy8b(src, srcIndex + 17U, dst, dstIndex + 2U);
	  copy1b(src, srcIndex + 16U, dst, dstIndex + 10U);
	  copy16b(src, srcIndex, dst, dstIndex + 11U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy4b(src, srcIndex + 24U, dst, dstIndex);
	  copy8b(src, srcIndex + 16U, dst, dstIndex + 4U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 12U);
	  copy8b(src, srcIndex, dst, dstIndex + 20U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 28U, dst, dstIndex);
	  copy4b(src, srcIndex + 24U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 16U, dst, dstIndex + 5U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 13U);
	  copy8b(src, srcIndex, dst, dstIndex + 21U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 28U, dst, dstIndex);
	  copy4b(src, srcIndex + 24U, dst, dstIndex + 2U);
	  copy8b(src, srcIndex + 16U, dst, dstIndex + 6U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 14U);
	  copy8b(src, srcIndex, dst, dstIndex + 22U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 30U, dst, dstIndex);
	  copy4b(src, srcIndex + 26U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 18U, dst, dstIndex + 5U);
	  copy8b(src, srcIndex + 10U, dst, dstIndex + 13U);
	  copy8b(src, srcIndex + 2U, dst, dstIndex + 21U);
	  copy2b(src, srcIndex, dst, dstIndex + 29U);
	}
	};

	protected static void copy(byte[] src, uint srcIndex, byte[] dst, uint dstIndex, uint length)
	{
		for (; length >= 32U; length -= 32U)
		{
			copy4b(src, srcIndex + 24U, dst, dstIndex);
			copy4b(src, srcIndex + 28U, dst, dstIndex + 4U);
			copy4b(src, srcIndex + 16U, dst, dstIndex + 8U);
			copy4b(src, srcIndex + 20U, dst, dstIndex + 12U);
			copy4b(src, srcIndex + 8U, dst, dstIndex + 16U);
			copy4b(src, srcIndex + 12U, dst, dstIndex + 20U);
			copy4b(src, srcIndex, dst, dstIndex + 24U);
			copy4b(src, srcIndex + 4U, dst, dstIndex + 28U);

			srcIndex += 32U;
			dstIndex += 32U;
		}
		if (length <= 0U)
			return;

		_copyMethods[(int)length](src, srcIndex, dst, dstIndex);
	}

	private static void copy16b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
	{
		copy8b(src, srcIndex + 8U, dst, dstIndex);
		copy8b(src, srcIndex, dst, dstIndex + 8U);
	}

	private static void copy1b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
	{
		dst[(int)dstIndex] = src[(int)srcIndex];
	}

	private static void copy2b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
	{
		dst[(int)dstIndex] = src[(int)srcIndex + 1];
		dst[(int)dstIndex + 1] = src[(int)srcIndex];
	}

	private static void copy3b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
	{
		dst[(int)dstIndex] = src[(int)srcIndex + 2];
		dst[(int)dstIndex + 1] = src[(int)srcIndex + 1];
		dst[(int)dstIndex + 2] = src[(int)srcIndex];
	}

	private static void copy4b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
	{
		dst[(int)dstIndex] = src[(int)srcIndex];
		dst[(int)dstIndex + 1] = src[(int)srcIndex + 1];
		dst[(int)dstIndex + 2] = src[(int)srcIndex + 2];
		dst[(int)dstIndex + 3] = src[(int)srcIndex + 3];
	}

	private static void copy8b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
	{
		copy4b(src, srcIndex, dst, dstIndex);
		copy4b(src, srcIndex + 4U, dst, dstIndex + 4U);
	}
}
