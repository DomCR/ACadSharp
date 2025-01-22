using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.XData;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.XData
{
	public class ExtendedDataTests
	{
		[Fact]
		public void AddObjectWithXDataTest()
		{
			ExtendedData data = new ExtendedData();

			data.AddControlStrings();

			//Empty
			var first = data.Records.First();
			Assert.IsType<ExtendedDataControlString>(first);
			Assert.False((first as ExtendedDataControlString).IsClosing);

			var last = data.Records.Last();
			Assert.IsType<ExtendedDataControlString>(last);
			Assert.True((last as ExtendedDataControlString).IsClosing);

			data.Records.Clear();

			//1 Item
			data.Records.Add(new ExtendedDataString("Hello"));

			data.AddControlStrings();

			first = data.Records.First();
			Assert.IsType<ExtendedDataControlString>(first);
			Assert.False((first as ExtendedDataControlString).IsClosing);

			last = data.Records.Last();
			Assert.IsType<ExtendedDataControlString>(last);
			Assert.True((last as ExtendedDataControlString).IsClosing);

			Assert.Equal(3, data.Records.Count);
		}

		[Fact]
		public void AddObjectWithXData()
		{
			CadDocument doc = new CadDocument();

			string appName = "my_custom_app";
			AppId app = new AppId(appName);
			Line line = new Line();
			ExtendedData extendedData = new ExtendedData();
			ExtendedDataRecord extendedDataRecord = new ExtendedDataString("extended data record");
			extendedData.Records.Add(extendedDataRecord);

			line.ExtendedData.Add(app, extendedData);

			doc.Entities.Add(line);

			Assert.Contains(app, doc.AppIds);
			Assert.False(app.Handle == 0);

			Assert.NotEmpty(line.ExtendedData);
			Assert.NotEmpty(line.ExtendedData.Get(appName).Records);
			Assert.Equal(extendedDataRecord, line.ExtendedData.Get(appName).Records.First());
		}

		[Fact]
		public void RemoveObjectWithXData()
		{
			CadDocument doc = new CadDocument();

			string appName = "my_custom_app";
			AppId app = new AppId(appName);
			Line line = new Line();
			ExtendedData extendedData = new ExtendedData();
			ExtendedDataRecord extendedDataRecord = new ExtendedDataString("extended data record");
			extendedData.Records.Add(extendedDataRecord);

			line.ExtendedData.Add(app, extendedData);

			doc.Entities.Add(line);

			doc.Entities.Remove(line);

			//Doc keeps the appid
			Assert.Contains(app, doc.AppIds);
			Assert.False(app.Handle == 0);

			Assert.NotEmpty(line.ExtendedData);
			Assert.True(line.ExtendedData.ContainsKeyName("my_custom_app"));

			foreach (var item in line.ExtendedData)
			{
				Assert.True(item.Key.Handle == 0);
				Assert.Null(item.Key.Document);
			}

			Assert.NotEmpty(line.ExtendedData.Get(appName).Records);
			Assert.Equal(extendedDataRecord, line.ExtendedData.Get(appName).Records.First());
		}
	}
}
