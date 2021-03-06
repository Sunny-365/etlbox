using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.DataFlow.Connectors;
using ETLBox.Exceptions;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using System;
using System.Collections.Generic;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class DbDestinationExceptionTests
    {
        public static SqlConnectionManager SqlConnection => Config.SqlConnection.ConnectionManager("DataFlow");

        public DbDestinationExceptionTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Fact]
        public void UnknownTable()
        {
            //Arrange
            string[] data = { "1", "2" };
            MemorySource<string[]> source = new MemorySource<string[]>();
            source.DataAsList.Add(data);
            DbDestination<string[]> dest = new DbDestination<string[]>(SqlConnection, "UnknownTable");
            source.LinkTo(dest);

            //Act & Assert
            Assert.Throws<ETLBoxException>(() =>
            {
                try
                {
                    source.Execute();
                    dest.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [Fact]
        public void UnknownTableViaTableDefinition()
        {
            //Arrange
            TableDefinition def = new TableDefinition("UnknownTable",
                new List<TableColumn>()
                {
                    new TableColumn("id", "INT")
                });

            //Arrange
            string[] data = { "1", "2" };
            MemorySource<string[]> source = new MemorySource<string[]>();
            source.DataAsList.Add(data);
            DbDestination<string[]> dest = new DbDestination<string[]>()
            {
                ConnectionManager = SqlConnection,
                DestinationTableDefinition = def
            };
            source.LinkTo(dest);

            //Act & Assert
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                try
                {
                    source.Execute();
                    dest.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }
    }
}
