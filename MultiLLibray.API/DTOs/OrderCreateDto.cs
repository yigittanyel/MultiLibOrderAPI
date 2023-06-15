namespace MultiLLibray.API.DTOs;

public record OrderCreateDto(string productName,int quantity,int totalPrice)
{
}
