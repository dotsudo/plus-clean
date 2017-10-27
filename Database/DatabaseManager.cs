namespace Plus.Database
{
    using System;
    using Core;
    using Interfaces;
    using MySql.Data.MySqlClient;

    public sealed class DatabaseManager
    {
        private readonly string _connectionStr;

        public DatabaseManager(string connectionStr) => _connectionStr = connectionStr;

        public bool IsConnected()
        {
            try
            {
                var con = new MySqlConnection(_connectionStr);
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT 1+1";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (MySqlException)
            {
                return false;
            }

            return true;
        }

        public IQueryAdapter GetQueryReactor()
        {
            try
            {
                IDatabaseClient dbConnection = new DatabaseConnection(_connectionStr);
                dbConnection.Connect();
                return dbConnection.GetQueryReactor();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
                return null;
            }
        }
    }
}