using BenchmarkDotNet.Configs;
using MultiLLibray.API.Context;
using MultiLLibray.API.DTOs;
using MultiLLibray.API.MapperProfiles;
using MultiLLibray.API.Models;
using MultiLLibray.API.Repositories;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using NLog;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace MultiLLibray.API.Benchmark;

public class BenchmarkService
{
    public class Config : ManualConfig
    {
        public Config()
        {
            SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
        }
    }

    [Benchmark(Baseline = true)]
    public List<Order> GetAllWithEF()
    {
        ApplicationDbContext context = new ApplicationDbContext();
        return context.Orders.AsNoTracking().ToList();
    }

    [Benchmark]
    public List<Order> GetAllWithDapper() 
    {
        string cnnStr = "Server=.;Database=MultiLibray;TrustServerCertificate=True;Encrypt=False;Trusted_Connection=True;Trusted_Connection=True";

        using (var connection = new SqlConnection(cnnStr))
        {
            connection.Open();
            string query = "SELECT * FROM Orders";
            return connection.Query<Order>(query).ToList();
        }
    }
}

