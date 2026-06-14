using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;


namespace SinkholeLibrary
{
    public static class DatabaseHandler
    {
        // The path to the SQLite database file. It is located in the parent directory of the application.
        public static string HEAD_PATH = $"{Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName}";
        public static string DB_PATH = $"Data Source={HEAD_PATH}\\personalSinkhole.db";

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

                CREATE TABLE IF NOT EXISTS logs (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date        TEXT NOT NULL,
                    Time        TEXT NOT NULL,
                    Domain      TEXT NOT NULL,
                    QueryType   TEXT,
                    WasBlocked  INTEGER NOT NULL,  -- 0 or 1
                    ResponseMs  INTEGER
                );
            """;
            cmd.ExecuteNonQuery();

            //will take a while
            //DatabaseHandler.AddTextFileToDB($"{HEAD_PATH}\\blocklist.txt");
            DatabaseHandler.AddTextFileToDB($"{HEAD_PATH}\\adblocklist.txt");
            DatabaseHandler.AddTextFileToDB($"{HEAD_PATH}\\personalblocklist.txt");
        }

        public static void AddNewDomain(string domain)
        {
            if (DatabaseHandler.IsDomain(domain))
            {
                return;
            }

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
            if(!DatabaseHandler.IsDomain(domain))
            {
                return;
            }

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

        public static void AddTextFileToDB(string dest)
        {
            using (FileStream fileStream = new FileStream(dest, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while(!reader.EndOfStream)
                    {
                        string textContent = reader.ReadLine();
                        textContent = textContent.Replace("0.0.0.0 ", string.Empty);
                        if (textContent != string.Empty && !IsDomain(textContent))
                        {
                            DatabaseHandler.AddNewDomain(textContent);
                        }
                    }
                }
            }
        }

        public static async void AddLogAsync(string domain, string type, int isBlocked, int responseTime)
        {
            using var con = new SqliteConnection(DB_PATH);
            con.OpenAsync();
            using var cmd = con.CreateCommand();    
            cmd.CommandText = """
                INSERT INTO logs (Date, Time, Domain, QueryType, WasBlocked, ResponseMs) VALUES ($Date, $Time, $Domain, $Querytype, $WasBlocked, $ResponseMs);
            """;
            cmd.Parameters.AddWithValue("$Date", DateTime.UtcNow.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("$Time", DateTime.UtcNow.ToString("HH:mm:ss"));
            cmd.Parameters.AddWithValue("$Domain", domain);
            cmd.Parameters.AddWithValue("$Querytype", type);
            cmd.Parameters.AddWithValue("$WasBlocked", isBlocked);
            cmd.Parameters.AddWithValue("$ResponseMs", responseTime);

            cmd.ExecuteNonQueryAsync();
        }
    }
}
