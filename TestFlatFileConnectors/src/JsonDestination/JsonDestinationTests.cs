using ETLBox.Connection;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using System.IO;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class JsonDestinationTests
    {
        public SqlConnectionManager SqlConnection => Config.SqlConnection.ConnectionManager("DataFlow");
        public JsonDestinationTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        public class MySimpleRow
        {
            public string Col2 { get; set; }
            public int Col1 { get; set; }
        }

        [Fact]
        public void SimpleFlowWithObject()
        {
            //Arrange
            TwoColumnsTableFixture s2C = new TwoColumnsTableFixture("JsonDestSimple");
            s2C.InsertTestDataSet3();
            DbSource<MySimpleRow> source = new DbSource<MySimpleRow>(SqlConnection, "JsonDestSimple");

            //Act
            JsonDestination<MySimpleRow> dest = new JsonDestination<MySimpleRow>("./SimpleWithObject.json", ResourceType.File);
            source.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            Assert.Equal(File.ReadAllText("res/JsonDestination/TwoColumnsSet3.json")
                , File.ReadAllText("./SimpleWithObject.json"));
        }
    }
}
