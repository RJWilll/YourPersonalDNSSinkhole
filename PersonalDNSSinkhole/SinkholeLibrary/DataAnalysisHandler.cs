using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

public class DataAnalysisHandler
{
    private readonly string _dbPath;

    public DataAnalysisHandler(string dbPath)
    {
        _dbPath = dbPath;
    }

    private SqliteConnection GetConnection()
    {
        var con = new SqliteConnection(_dbPath);
        con.Open();
        return con;
    }

    public double GetAverageQueriesPerDay()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT AVG(daily_count) FROM (
                SELECT COUNT(*) AS daily_count
                FROM logs
                GROUP BY DATE(Timestamp)
            )";
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? 0 : Convert.ToDouble(result);
    }

    public long GetTotalQueries()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM logs";
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public List<(int Hour, long Count)> GetPeakUsageTimes()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT CAST(strftime('%H', Timestamp) AS INTEGER) AS hour, COUNT(*) AS count
            FROM logs
            GROUP BY hour
            ORDER BY hour";

        var results = new List<(int, long)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add((reader.GetInt32(0), reader.GetInt64(1)));

        return results;
    }

    public double GetBlockRate()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                100.0 * SUM(WasBlocked) / COUNT(*) 
            FROM logs";
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? 0 : Convert.ToDouble(result);
    }

    public List<(string Date, double BlockRate)> GetBlockRateOverTime()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                DATE(Timestamp) AS date,
                100.0 * SUM(WasBlocked) / COUNT(*) AS block_rate
            FROM logs
            GROUP BY date
            ORDER BY date";

        var results = new List<(string, double)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add((reader.GetString(0), reader.GetDouble(1)));

        return results;
    }

    public List<(string Domain, long Count)> GetTopQueriedDomains(int topN = 10)
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT Domain, COUNT(*) AS count
            FROM logs
            GROUP BY Domain
            ORDER BY count DESC
            LIMIT $topN";
        cmd.Parameters.AddWithValue("$topN", topN);

        var results = new List<(string, long)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add((reader.GetString(0), reader.GetInt64(1)));

        return results;
    }

    public (string Timestamp, string Domain)? GetLastQuery()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT Timestamp, Domain
            FROM logs
            ORDER BY Timestamp DESC
            LIMIT 1";

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (reader.GetString(0), reader.GetString(1));

        return null;
    }

    public long GetUniqueDomainsToday()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(DISTINCT Domain)
            FROM logs
            WHERE DATE(Timestamp) = DATE('now')";
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public List<(int Hour, long Count)> GetTopQueryTimes(int topN = 5)
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT CAST(strftime('%H', Timestamp) AS INTEGER) AS hour, COUNT(*) AS count
            FROM logs
            GROUP BY hour
            ORDER BY count DESC
            LIMIT $topN";
        cmd.Parameters.AddWithValue("$topN", topN);

        var results = new List<(int, long)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add((reader.GetInt32(0), reader.GetInt64(1)));

        return results;
    }

    public double GetAverageResponseTime()
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT AVG(ResponseMs)
            FROM logs
            WHERE WasBlocked = 0 AND ResponseMs >= 0";
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? 0 : Convert.ToDouble(result);
    }

    public List<(string Domain, double AvgMs)> GetSlowestDomains(int topN = 10)
    {
        using var con = GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT Domain, AVG(ResponseMs) AS avg_ms
            FROM logs
            WHERE WasBlocked = 0 AND ResponseMs >= 0
            GROUP BY Domain
            ORDER BY avg_ms DESC
            LIMIT $topN";
        cmd.Parameters.AddWithValue("$topN", topN);

        var results = new List<(string, double)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add((reader.GetString(0), reader.GetDouble(1)));

        return results;
    }
}