using ETLBox.Connection;
using ETLBox.DataFlow.Connectors;
using ETLBox.DataFlow.Transformations;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using System.Dynamic;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class RowTransformationDynamicObjectTests
    {
        public SqlConnectionManager Connection => Config.SqlConnection.ConnectionManager("DataFlow");
        public RowTransformationDynamicObjectTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Fact]
        public void ConvertIntoObject()
        {
            //Arrange
            TwoColumnsTableFixture dest2Columns = new TwoColumnsTableFixture("DestinationRowTransformationDynamic");
            CsvSource<ExpandoObject> source = new CsvSource<ExpandoObject>("res/RowTransformation/TwoColumns.csv");

            //Act
            RowTransformation<ExpandoObject> trans = new RowTransformation<ExpandoObject>(
                csvdata =>
                {
                    dynamic c = csvdata as ExpandoObject;
                    c.Col1 = c.Header1;
                    c.Col2 = c.Header2;
                    return c;
                });
            DbDestination<ExpandoObject> dest = new DbDestination<ExpandoObject>(Connection, "DestinationRowTransformationDynamic");
            source.LinkTo(trans);
            trans.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            dest2Columns.AssertTestData();
        }
    }
}
