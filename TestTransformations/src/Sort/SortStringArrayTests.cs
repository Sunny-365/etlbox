using ETLBox.Connection;
using ETLBox.DataFlow.Connectors;
using ETLBox.DataFlow.Transformations;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class SortStringArrayTests
    {
        public SqlConnectionManager Connection => Config.SqlConnection.ConnectionManager("DataFlow");
        public SortStringArrayTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Fact]
        public void SortSimpleDataDescending()
        {
            //Arrange
            TwoColumnsTableFixture source2Columns = new TwoColumnsTableFixture("SortSourceNonGeneric");
            source2Columns.InsertTestData();
            DbSource<string[]> source = new DbSource<string[]>(Connection, "SortSourceNonGeneric");

            //Act
            List<string[]> actual = new List<string[]>();
            CustomDestination<string[]> dest = new CustomDestination<string[]>(
                row => actual.Add(row)
            );
            Comparison<string[]> comp = new Comparison<string[]>(
                   (x, y) => int.Parse(y[0]) - int.Parse(x[0])
                );
            Sort<string[]> block = new Sort<string[]>(comp);
            source.LinkTo(block);
            block.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            List<int> expected = new List<int>() { 3, 2, 1 };
            Assert.Equal(expected, actual.Select(row => int.Parse(row[0])).ToList());
        }
    }
}
