using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow.Connectors;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class DbDestinationDynamicObjectTests
    {
        public static IEnumerable<object[]> Connections => Config.AllSqlConnections("DataFlow");
        public static IEnumerable<object[]> ConnectionsNoSQLite => Config.AllConnectionsWithoutSQLite("DataFlow");

        public DbDestinationDynamicObjectTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Theory, MemberData(nameof(Connections))]
        public void SourceMoreColumnsThanDestination(IConnectionManager connection)
        {
            //Arrange
            FourColumnsTableFixture source4Columns = new FourColumnsTableFixture(connection, "SourceDynamic4Cols");
            source4Columns.InsertTestData();
            TwoColumnsTableFixture dest2Columns = new TwoColumnsTableFixture(connection, "DestinationDynamic2Cols");

            //Act
            DbSource<ExpandoObject> source = new DbSource<ExpandoObject>(connection, "SourceDynamic4Cols");
            DbDestination<ExpandoObject> dest = new DbDestination<ExpandoObject>(connection, "DestinationDynamic2Cols");

            source.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            dest2Columns.AssertTestData();
        }

        [Theory, MemberData(nameof(Connections))]
        public void DestinationMoreColumnsThanSource(IConnectionManager connection)
        {
            //Arrange
            TwoColumnsTableFixture source2Columns = new TwoColumnsTableFixture(connection, "SourceDynamicDiffCols");
            source2Columns.InsertTestData();
            CreateTableTask.Create(connection, "DestinationDynamicDiffCols",
                new List<TableColumn>() {
                    new TableColumn("Col5", "VARCHAR(100)", allowNulls:true),
                    new TableColumn("Col2", "VARCHAR(100)",allowNulls:true),
                    new TableColumn("Col1", "INT",allowNulls:true),
                    new TableColumn("ColX", "INT",allowNulls:true),
                });

            //Act
            DbSource<ExpandoObject> source = new DbSource<ExpandoObject>(connection, "SourceDynamicDiffCols");
            DbDestination<ExpandoObject> dest = new DbDestination<ExpandoObject>(connection, "DestinationDynamicDiffCols");

            source.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            string QB = connection.QB;
            string QE = connection.QE;
            Assert.Equal(3, RowCountTask.Count(connection, "DestinationDynamicDiffCols"));
            Assert.Equal(1, RowCountTask.Count(connection, "DestinationDynamicDiffCols", $"{QB}Col1{QE} = 1 AND {QB}Col2{QE}='Test1' AND {QB}Col5{QE} IS NULL AND {QB}ColX{QE} IS NULL"));
            Assert.Equal(1, RowCountTask.Count(connection, "DestinationDynamicDiffCols", $"{QB}Col1{QE} = 2 AND {QB}Col2{QE}='Test2' AND {QB}Col5{QE} IS NULL AND {QB}ColX{QE} IS NULL"));
            Assert.Equal(1, RowCountTask.Count(connection, "DestinationDynamicDiffCols", $"{QB}Col1{QE} = 3 AND {QB}Col2{QE}='Test3' AND {QB}Col5{QE} IS NULL AND {QB}ColX{QE} IS NULL"));
        }

        [Theory, MemberData(nameof(ConnectionsNoSQLite))]
        public void DestinationWithIdentityColumn(IConnectionManager connection)
        {
            //Arrange
            TwoColumnsTableFixture source2Columns = new TwoColumnsTableFixture(connection, "SourceDynamicIDCol");
            source2Columns.InsertTestData();
            CreateTableTask.Create(connection, "DestinationDynamicIdCol",
                new List<TableColumn>() {
                    new TableColumn("Id", "BIGINT", allowNulls:false, isPrimaryKey:true, isIdentity:true),
                    new TableColumn("Col2", "VARCHAR(100)",allowNulls:true),
                    new TableColumn("Col1", "INT",allowNulls:true),
                    new TableColumn("ColX", "INT",allowNulls:true),
                });

            //Act
            DbSource<ExpandoObject> source = new DbSource<ExpandoObject>(connection, "SourceDynamicIDCol");
            DbDestination<ExpandoObject> dest = new DbDestination<ExpandoObject>(connection, "DestinationDynamicIdCol");

            source.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            string QB = connection.QB;
            string QE = connection.QE;
            Assert.Equal(3, RowCountTask.Count(connection, "DestinationDynamicIdCol"));
            Assert.Equal(1, RowCountTask.Count(connection, "DestinationDynamicIdCol", $"{QB}Col1{QE} = 1 AND {QB}Col2{QE}='Test1' AND {QB}Id{QE} > 0 AND {QB}ColX{QE} IS NULL"));
            Assert.Equal(1, RowCountTask.Count(connection, "DestinationDynamicIdCol", $"{QB}Col1{QE} = 2 AND {QB}Col2{QE}='Test2' AND {QB}Id{QE} > 0 AND {QB}ColX{QE} IS NULL"));
            Assert.Equal(1, RowCountTask.Count(connection, "DestinationDynamicIdCol", $"{QB}Col1{QE} = 3 AND {QB}Col2{QE}='Test3' AND {QB}Id{QE} > 0 AND {QB}ColX{QE} IS NULL"));
        }
    }
}
