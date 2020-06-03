﻿using ETLBox;
using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow;
using ETLBoxTests.Helper;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ETLBoxTests.Performance
{
    [Collection("Performance")]
    public class DbMergeDeleteTests
    {
        private readonly ITestOutputHelper output;

        SqlConnectionManager SqlConnection => Config.SqlConnection.ConnectionManager("Performance");

        public DbMergeDeleteTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        public class MyMergeRow : MergeableRow
        {
            [IdColumn]
            public Guid Id { get; set; }

            [CompareColumn]
            public DateTimeOffset LastUpdated { get; set; }

            public string Value { get; set; }
        }

        [Theory,
            InlineData(10000, 5000)]
        public void NoUpdateWithGuid(int rowsInDest, int rowsInSource)
        {
            //Arrange
            CreateDestinationTable("MergeDestination");
            List<MyMergeRow> knownGuids = CreateTestData(rowsInSource);
            TransferTestDataIntoDestination(knownGuids);
            MemorySource<MyMergeRow> source = AddNewTestData(rowsInDest, knownGuids);
            DbMerge<MyMergeRow> mergeDest = new DbMerge<MyMergeRow>(SqlConnection, "MergeDestination");
            source.LinkTo(mergeDest);

            //Act
            var executionTime = BigDataHelper.LogExecutionTime("Execute merge", () =>
            {
                source.Execute();
                mergeDest.Wait();
            });

            //Assert
            Assert.Equal(rowsInDest + rowsInSource, RowCountTask.Count(SqlConnection, "MergeDestination") ?? 0);
            Assert.True(executionTime <= TimeSpan.FromMilliseconds((rowsInDest + rowsInSource) * 2));
        }

        private void CreateDestinationTable(string tableName)
        {
            DropTableTask.DropIfExists(SqlConnection, tableName);
            CreateTableTask.Create(SqlConnection, tableName,
                new List<TableColumn>()
                {
                    new TableColumn("Id", "UNIQUEIDENTIFIER", allowNulls: false, isPrimaryKey: true),
                    new TableColumn("LastUpdated","DATETIMEOFFSET", allowNulls: false),
                    new TableColumn("Value","CHAR(1)", allowNulls: false),
                });
        }

        private static List<MyMergeRow> CreateTestData(int rowsInSource)
        {
            List<MyMergeRow> knownGuids = new List<MyMergeRow>();
            for (int i = 0; i < rowsInSource; i++)
                knownGuids.Add(new MyMergeRow()
                {
                    Id = Guid.NewGuid(),
                    LastUpdated = DateTime.Now,
                    Value = TestHashHelper.RandomString(1)
                });
            return knownGuids;
        }

        private void TransferTestDataIntoDestination(List<MyMergeRow> knownGuids)
        {
            MemorySource<MyMergeRow> source = new MemorySource<MyMergeRow>();
            source.DataAsList = knownGuids;
            DbDestination<MyMergeRow> dest = new DbDestination<MyMergeRow>(SqlConnection, "MergeDestination");
            source.LinkTo(dest);
            source.Execute();
            dest.Wait();
        }

        private MemorySource<MyMergeRow> AddNewTestData(int rowsInDest, List<MyMergeRow> knownGuids)
        {
            MemorySource<MyMergeRow> source = new MemorySource<MyMergeRow>();
            source.DataAsList = knownGuids;
            for (int i = 0; i < rowsInDest; i++)
                knownGuids.Add(new MyMergeRow()
                {
                    Id = Guid.NewGuid(),
                    LastUpdated = DateTime.Now,
                    Value = TestHashHelper.RandomString(1)
                });
            return source;
        }
    }
}
