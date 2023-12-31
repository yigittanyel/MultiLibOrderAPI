﻿using Mapster;
using MultiLLibray.API.DTOs;
using MultiLLibray.API.Models;

namespace MultiLLibray.API.MapperProfiles;
public class OrderMapperProfile
{
    public OrderMapperProfile()
    {
        TypeAdapterConfig<OrderCreateDto, Order>.NewConfig()
            .Map(dest => dest.ProductName, src => src.productName)
            .Map(dest => dest.Quantity, src => src.quantity)
            .Map(dest => dest.TotalPrice, src => src.totalPrice);
    }

    public Order Map(OrderCreateDto orderDto)
    {
        return orderDto.Adapt<Order>();
    }
}

public class UserMapperProfile
{
    public UserMapperProfile()
    {
        TypeAdapterConfig<UserDto, User>.NewConfig()
            .Map(dest => dest.Username, src => src.username)
            .Map(dest => dest.Password, src => src.password);
    }

    public User Map(UserDto userDto)
    {
        return userDto.Adapt<User>();
    }
}