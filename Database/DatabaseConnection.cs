﻿namespace Plus.Database
{
    using System;
    using System.Data;
    using Adapter;
    using Interfaces;
    using MySql.Data.MySqlClient;

    internal class DatabaseConnection : IDatabaseClient
    {
        private readonly IQueryAdapter _adapter;
        private readonly MySqlConnection _con;

        public DatabaseConnection(string connectionStr)
        {
            _con = new MySqlConnection(connectionStr);
            _adapter = new NormalQueryReactor(this);
        }

        public void Connect()
        {
            if (_con.State != ConnectionState.Closed)
            {
                return;
            }

            try
            {
                _con.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Disconnect()
        {
            if (_con.State == ConnectionState.Open)
            {
                _con.Close();
            }
        }

        public IQueryAdapter GetQueryReactor() => _adapter;

        public void ReportDone()
        {
            Dispose();
        }

        public MySqlCommand CreateNewCommand() => _con.CreateCommand();

        public void Dispose()
        {
            if (_con.State == ConnectionState.Open)
            {
                _con.Close();
            }
            _con.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Prepare()
        {
            // nothing here
        }
    }
}