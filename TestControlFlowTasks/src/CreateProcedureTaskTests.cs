using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using ETLBox.Exceptions;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using System.Collections.Generic;
using Xunit;

namespace ETLBoxTests.ControlFlowTests
{
    [Collection("ControlFlow")]
    public class CreateProcedureTaskTests
    {
        public static IEnumerable<object[]> Connections => Config.AllConnectionsWithoutSQLite("ControlFlow");

        public CreateProcedureTaskTests(ControlFlowDatabaseFixture dbFixture)
        { }

        [Theory, MemberData(nameof(Connections))]
        public void CreateProcedure(IConnectionManager connection)
        {
            //Arrange
            //Act
            CreateProcedureTask.CreateOrAlter(connection, "Proc1", "SELECT 1;");
            //Assert
            Assert.True(IfProcedureExistsTask.IsExisting(connection, "Proc1"));
        }

        [Theory, MemberData(nameof(Connections))]
        public void AlterProcedure(IConnectionManager connection)
        {
            //Arrange
            CreateProcedureTask.CreateOrAlter(connection, "Proc2", "SELECT 1;");
            Assert.True(IfProcedureExistsTask.IsExisting(connection, "Proc2"));

            //Act
            CreateProcedureTask.CreateOrAlter(connection, "Proc2", "SELECT 5;");

            //Assert
            Assert.True(IfProcedureExistsTask.IsExisting(connection, "Proc2"));
        }

        [Theory, MemberData(nameof(Connections))]
        public void CreateProcedureWithParameter(IConnectionManager connection)
        {
            //Arrange
            List<ProcedureParameter> pars = new List<ProcedureParameter>() {
                new ProcedureParameter("Par1", "VARCHAR(10)"),
                new ProcedureParameter("Par2", "INT", "7"),
            };
            //Act
            CreateProcedureTask.CreateOrAlter(connection, "Proc3", "SELECT 1;", pars);
            //Assert
            Assert.True(IfProcedureExistsTask.IsExisting(connection, "Proc3"));
        }

        [Theory, MemberData(nameof(Connections))]
        public void CreatProcedureWithProcedureObject(IConnectionManager connection)
        {
            //Arrange
            List<ProcedureParameter> pars = new List<ProcedureParameter>() {
                new ProcedureParameter("Par1", "varchar(10)"),
                new ProcedureParameter("Par2", "int", "7"),
            };
            ProcedureDefinition procDef = new ProcedureDefinition("Proc4", "SELECT 1;", pars);
            //Act
            CreateProcedureTask.CreateOrAlter(connection, procDef);
            //Assert
            Assert.True(IfProcedureExistsTask.IsExisting(connection, "Proc4"));
        }

        [Fact]
        public void NotSupportedWithSQLite()
        {
            Assert.Throws<ETLBoxNotSupportedException>(
                () => CreateDatabaseTask.Create(Config.SQLiteConnection.ConnectionManager("ControlFlow"), "Test")
                );
        }
    }
}
