using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiLLibray.API.Context;
using MultiLLibray.API.DTOs;
using MultiLLibray.API.MapperProfiles;
using MultiLLibray.API.Models;
using MultiLLibray.API.Repositories;

namespace MultiLLibray.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OrderRepository _orderRepository;
        private readonly OrderMapperProfile _orderMapper;
        public OrderController(ApplicationDbContext dbContext, OrderRepository orderRepository, OrderMapperProfile orderMapper)
        {
            _dbContext = dbContext;
            _orderRepository = orderRepository;
            _orderMapper = orderMapper;
        }

        [HttpGet("[action]")]
        public ActionResult<List<Order>> GetAllOrders()
        {
            var orders = _orderRepository.GetAllOrders();
            return Ok(orders);
        }

        [HttpPost("[action]")]
        public IActionResult CreateOrder(OrderCreateDto orderDto)
        {
            Order order = _orderMapper.Map(orderDto);
            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateSeedData()
        {
            for(int i = 1; i <= 100; i++)
            {
                OrderCreateDto orderCreateDto = new($"pencil{i}", i * 5, i * 10);
                Order order = _orderMapper.Map(orderCreateDto);
                _dbContext.Orders.Add(order);
            }

            _dbContext.SaveChanges();
            return Ok();
        }
    }
}
