using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSUtilities.IO
{
	#region Regex reader (to check)
	//public static class Csv
	//{
	//	private const string QUOTE = "\"";
	//	private const string ESCAPED_QUOTE = "\"\"";
	//	private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };
	//	public static string Escape(string s)
	//	{
	//		if (s.Contains(QUOTE))
	//			s = s.Replace(QUOTE, ESCAPED_QUOTE);

	//		if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
	//			s = QUOTE + s + QUOTE;

	//		return s;
	//	}
	//	public static string Unescape(string s)
	//	{
	//		if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
	//		{
	//			s = s.Substring(1, s.Length - 2);

	//			if (s.Contains(ESCAPED_QUOTE))
	//				s = s.Replace(ESCAPED_QUOTE, QUOTE);
	//		}

	//		return s;
	//	}
	//}

	//public sealed class _CsvReader_ : System.IDisposable
	//{
	//	public long RowIndex { get { return m_rownumber; } }
	//	private long m_rownumber = 0;
	//	private TextReader m_reader;
	//	private static Regex rexCsvSplitter = new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
	//	private static Regex rexRunOnLine = new Regex(@"^[^""]*(?:""[^""]*""[^""]*)*""[^""]*$");

	//	public _CsvReader_(string path) : this(new FileStream(path, FileMode.Open, FileAccess.Read)) { }
	//	public _CsvReader_(Stream stream)
	//	{
	//		m_reader = new StreamReader(stream);
	//	}

	//	public System.Collections.IEnumerable RowEnumerator
	//	{
	//		get
	//		{
	//			if (null == m_reader)
	//				throw new System.ApplicationException("I can't start reading without CSV input.");

	//			m_rownumber = 0;
	//			string sLine;
	//			string sNextLine;

	//			while (null != (sLine = m_reader.ReadLine()))
	//			{
	//				while (rexRunOnLine.IsMatch(sLine) && null != (sNextLine = m_reader.ReadLine()))
	//					sLine += "\n" + sNextLine;

	//				m_rownumber++;
	//				string[] values = rexCsvSplitter.Split(sLine);

	//				for (int i = 0; i < values.Length; i++)
	//					values[i] = Csv.Unescape(values[i]);

	//				yield return values;
	//			}

	//			m_reader.Close();
	//		}

	//	}
	//	public void Dispose()
	//	{
	//		if (null != m_reader) m_reader.Dispose();
	//	}
	//}
	#endregion

	internal class CsvReader : IDisposable
	{
		public char Separator { get; set; }
		public string[] Headers { get; }
		public bool HasHeaders { get; }

		public StreamReader m_reader;
		public CsvReader(string path, char separator, bool hasHeaders)
		{
			m_reader = new StreamReader(path);

			HasHeaders = hasHeaders;
			Separator = separator;

			if (hasHeaders)
			{
				//Get the headers of the current csv
				Headers = m_reader.ReadLine().ToArgs(separator, ignoreEmpty: false);

				if (Headers.Contains(string.Empty))
					throw new FormatException($"Empty header found.");
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="separator"></param>
		/// <param name="hasHeaders"></param>
		/// <returns></returns>
		/// <exception cref="FormatException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="DuplicateNameException"></exception>
		/// <exception cref="InvalidExpressionException"></exception>
		public static DataTable ReadAsTable(IEnumerable<string> lines, char separator, bool hasHeaders)
		{
			DataTable table = new DataTable();
			int toSkip = 0;

			//Setup the columns
			foreach (string header in lines.First().ToArgs(separator, ignoreEmpty: false))
			{
				if (hasHeaders)
				{
					if(string.IsNullOrEmpty(header))
						throw new FormatException($"Empty header found.");

					table.Columns.Add(new DataColumn(header));
				}
				else
					table.Columns.Add(new DataColumn());
			}

			if (hasHeaders)
				toSkip++;

			//Feed the table
			foreach (var line in lines.Skip(toSkip))
			{
				table.Rows.Add(line.ToArgs(separator, ignoreEmpty: false));
			}

			return table;
		}
		public DataTable ReadAsTable()
		{
			long initialPos = m_reader.BaseStream.Position;
			m_reader.BaseStream.Position = 0;

			//Feed the tables
			List<string> lines = new List<string>();
			string line;
			while (!string.IsNullOrEmpty(line = m_reader.ReadLine()))
			{
				lines.Add(line);
			}

			m_reader.BaseStream.Position = initialPos;

			return CsvReader.ReadAsTable(lines, Separator, HasHeaders);
		}
		public Dictionary<string, string> ReadRow()
		{
			throw new NotImplementedException();
		}
		public void Dispose()
		{

		}
	}
}
