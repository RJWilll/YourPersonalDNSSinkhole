using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;


namespace SinkholeLibrary
{
    public static class DatabaseHandler
    {
        // The path to the SQLite database file. It is located in the parent directory of the application.
        public static string DB_PATH = $"Data Source={Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName}\\personalSinkhole.db";

        // Initializes the database by creating necessary tables if they do not exist.
        public static void InitializeDatabase()
        {
            using var con = new SqliteConnection(DB_PATH);
            con.Open();

            var cmd = con.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS domains (
                    domain          TEXT NOT NULL PRIMARY KEY
                );
            """;
            cmd.ExecuteNonQuery();

        }

        public static void AddNewDomain(string domain)
        {
            using var con = new SqliteConnection(DB_PATH);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = """
                INSERT INTO domains (domain) VALUES ($domain);
            """;
            cmd.Parameters.AddWithValue("$domain", domain);
            cmd.ExecuteNonQuery();

        }

        public static void DeleteDomain(string domain)
        {
            using var con = new SqliteConnection(DB_PATH);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = """
                DELETE FROM domains WHERE domain = $domain;
            """;
            cmd.Parameters.AddWithValue("$domain", domain);
            cmd.ExecuteNonQuery();
        }

        public static bool IsDomain(string domain)
        {
            var con = new SqliteConnection(DB_PATH);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*) FROM domains WHERE domain = $domain;
            """;
            cmd.Parameters.AddWithValue("$domain", domain);
            cmd.ExecuteNonQuery();
            
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
    }
}
