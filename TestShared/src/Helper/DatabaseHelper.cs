﻿using ETLBox.Connection;
using ETLBox.ControlFlow.Tasks;

namespace ETLBoxTests.Helper
{
    public class DatabaseHelper
    {
        private static void DropAndCreate(IConnectionManager connManagerMaster, string dbName)
        {
            new DropDatabaseTask(dbName)
            {
                DisableLogging = true,
                ConnectionManager = connManagerMaster
            }.DropIfExists();

            new CreateDatabaseTask(dbName)
            {
                DisableLogging = true,
                ConnectionManager = connManagerMaster
            }.Execute();
        }

        public static void RecreateSqlDatabase(string section)
        {
            var connManagerMaster = new SqlConnectionManager(
                            Config.SqlConnection.ConnectionString(section).CloneWithMasterDbName()
                            );
            var dbName = Config.SqlConnection.ConnectionString(section).DbName;

            DropAndCreate(connManagerMaster, dbName);
        }

        public static void RecreateMySqlDatabase(string section)
        {
            var connManagerMaster = new MySqlConnectionManager(
                            Config.MySqlConnection.ConnectionString(section).CloneWithMasterDbName()
                            );
            var dbName = Config.MySqlConnection.ConnectionString(section).DbName;
            DropAndCreate(connManagerMaster, dbName);
        }

        public static void RecreateMariaDbDatabase(string section)
        {
            var connManagerMaster = new MariaDbConnectionManager(
                            Config.MariaDbConnection.ConnectionString(section).CloneWithMasterDbName()
                            );
            var dbName = Config.MariaDbConnection.ConnectionString(section).DbName;
            DropAndCreate(connManagerMaster, dbName);
        }

        public static void RecreatePostgresDatabase(string section)
        {
            var connManagerMaster = new PostgresConnectionManager(
                            Config.PostgresConnection.ConnectionString(section).CloneWithMasterDbName()
                            );
            var dbName = Config.PostgresConnection.ConnectionString(section).DbName;

            DropAndCreate(connManagerMaster, dbName);
        }

        public static void CleanUpOracle(string section)
        {
            var connManager = Config.OracleConnection.ConnectionManager(section);
            CleanUpSchemaTask.CleanUp(connManager);
        }
    }
}
