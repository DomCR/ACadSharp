using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.XData;
using Xunit;

namespace ACadSharp.Tests.XData
{
	public class ExtendedDataTests
	{
		[Fact]
		public void AddObjectWithXData()
		{
			CadDocument doc = new CadDocument();

			string appName = "my_custom_app";
			AppId app = new AppId(appName);
			Line line = new Line();
			ExtendedData extendedData = new ExtendedData();
			extendedData.Records.Add(new ExtendedDataString("extended data record"));


			line.ExtendedData.Add(app, extendedData);

			doc.Entities.Add(line);

			Assert.Contains(app, doc.AppIds);
			Assert.NotEmpty(line.ExtendedData);
			Assert.NotEmpty(line.ExtendedData.Get(appName).Records);
		}
	}
}
