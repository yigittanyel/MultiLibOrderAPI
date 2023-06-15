﻿using Dapper;
using Microsoft.Data.SqlClient;
using MultiLLibray.API.Models;

namespace MultiLLibray.API.Repositories;

public class OrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Order> GetAllOrders()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Orders";
            return connection.Query<Order>(query).ToList();
        }
    }

    public Order GetOrderById(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Orders WHERE Id = @Id";
            return connection.QuerySingleOrDefault<Order>(query, new { Id = id });
        }
    }

    public void CreateOrder(Order order)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "INSERT INTO Orders (CustomerName, OrderDate) VALUES (@CustomerName, @OrderDate)";
            connection.Execute(query, order);
        }
    }

    // Diğer CRUD işlemleri buraya eklenebilir
}
