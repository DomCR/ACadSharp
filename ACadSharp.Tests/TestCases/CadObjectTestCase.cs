using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.TestCases
{
	public class CadSystemVariablePair
	{
		public string Name { get; set; }
		public JToken Value { get; set; }

		public override string ToString()
		{
			return $"{Name}:{Value}";
		}
	}

	public class FileCase
	{
		public ACadVersion Version { get; set; }
		public List<CadObject> CadObjects { get; set; }

		public override string ToString()
		{
			return $"{Version}";
		}
	}

	public class DxfCodeValuePair
	{
		public DxfCode Code { get; set; }
		public object Value { get; set; }
	}

	public class CadObjectTestCase
	{
		public List<DxfCodeValuePair> Validation { get; set; } = new List<DxfCodeValuePair>();

		public ulong GetHandle()
		{
			var pair = Validation.FirstOrDefault(o => o.Code == DxfCode.Handle);
			if (pair == null)
				return 0;
			else
				return (ulong)Convert.ChangeType(pair.Value, typeof(ulong));

		}
		public void Compare(CadObject cad)
		{
			foreach (DxfCodeValuePair item in Validation)
			{

			}
		}
	}
}
