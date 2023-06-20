using Elasticsearch.Net;
using Iced.Intel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MultiLLibray.API.Context;
using MultiLLibray.API.DTOs;
using MultiLLibray.API.MapperProfiles;
using MultiLLibray.API.Models;
using MultiLLibray.API.Repositories;
using Newtonsoft.Json.Linq;
using Polly;

namespace MultiLLibray.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OrderRepository _orderRepository;
        private readonly OrderMapperProfile _orderMapper;
        private readonly Policy retryPolicy;
        private readonly RedisCacheHelper _cacheHelper;

        public OrdersController(ApplicationDbContext dbContext, OrderRepository orderRepository, OrderMapperProfile orderMapper, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _orderRepository = orderRepository;
            _orderMapper = orderMapper;
            _cacheHelper = new RedisCacheHelper(cache);

            retryPolicy = Policy.Handle<Exception>()
                .Retry(3, (exception, retryCount) =>
                {
                    var logger = NLog.LogManager.GetCurrentClassLogger();
                    logger.Error(exception, "Hata oluştu. Yeniden deneme sayısı: {RetryCount}", retryCount);
                });
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            var cachedOrders = await _cacheHelper.GetCacheValueAsync<List<Order>>("AllOrders");

            if (cachedOrders != null)
            {
                return Ok(cachedOrders);
            }

            var orders = _orderRepository.GetAllOrders();

            await _cacheHelper.SetCacheValueAsync("AllOrders", orders, TimeSpan.FromMinutes(10));

            return Ok(orders);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrder(OrderCreateDto orderDto)
        {
            try
            {
                await retryPolicy.Execute(async () =>
                {
                    Order order = _orderMapper.Map(orderDto);
                    _dbContext.Orders.Add(order);
                    _dbContext.SaveChanges();

                    await _cacheHelper.RemoveCacheValueAsync("AllOrders");
                });

                return Ok();
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "CreateOrder metodu içinde bir hata oluştu.");
                return StatusCode(500, "Bir hata oluştu.");
            }
        }

        [HttpPost("[action]")]
        public IActionResult CreateOrderDapper(OrderCreateDto orderDto)
        {
            try
            {
                retryPolicy.Execute(() =>
                {
                    Order order = _orderMapper.Map(orderDto);
                    _orderRepository.CreateOrder(order);

                });

                return Ok();
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "CreateOrder metodu içinde bir hata oluştu.");
                return StatusCode(500, "Bir hata oluştu.");
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateSeedData()
        {
            for (int i = 1; i <= 100; i++)
            {
                OrderCreateDto orderCreateDto = new($"pencil{i}", i * 5, i * 10);
                Order order = _orderMapper.Map(orderCreateDto);
                _dbContext.Orders.Add(order);
            }

            _dbContext.SaveChanges();

            // Tüm siparişlerin önbelleğini temizle
            await _cacheHelper.RemoveCacheValueAsync("AllOrders");

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SyncToElastic()
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));

            var client = new ElasticLowLevelClient(settings);

            List<Order> orders = await _dbContext.Orders.AsNoTracking().ToListAsync();

            var tasks = new List<Task>();

            foreach (var order in orders)
            {
                tasks.Add(client.IndexAsync<StringResponse>("orders", order.Id.ToString(), PostData.Serializable(new
                {
                    order.Id,
                    order.ProductName,
                    order.Quantity,
                    order.TotalPrice
                })));
            }

            await Task.WhenAll(tasks);

            return Ok();
        }

        [HttpGet("[action]/{value}")]
        public async Task<IActionResult> GetDataListWithEF(string value)
        {
            string cacheKey = $"Orders_{value}"; // Önbellek anahtarını değeri baz alarak oluşturun

            var cachedOrders = await _cacheHelper.GetCacheValueAsync<List<Order>>(cacheKey);

            if (cachedOrders != null)
            {
                var filteredOrders = cachedOrders.Where(p => p.ProductName.Contains(value)).Take(10);
                return Ok(filteredOrders);
            }

            IList<Order> orders =
                await _dbContext.Set<Order>()
                .Where(p => p.ProductName.Contains(value))
                .AsNoTracking()
                .ToListAsync();

            // Verileri önbelleğe kaydet
            await _cacheHelper.SetCacheValueAsync(cacheKey, orders, TimeSpan.FromMinutes(10));

            return Ok(orders.Take(10));
        }


        [HttpGet("[action]/{value}")]
        public async Task<IActionResult> GetDataListWithElasticSearch(string value)
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));

            var client = new ElasticLowLevelClient(settings);

            var response = await client.SearchAsync<StringResponse>("orders", PostData.Serializable(new
            {
                query = new
                {
                    wildcard = new
                    {
                        ProductName = new { value = $"*{value}*" }
                    }
                }
            }));

            var results = JObject.Parse(response.Body);

            var hits = results["hits"]["hits"].ToObject<List<JObject>>();

            List<Order> orders = new();

            foreach (var hit in hits)
            {
                orders.Add(hit["_source"].ToObject<Order>());
            }

            return Ok(orders.Take(10));
        }

    }
}
