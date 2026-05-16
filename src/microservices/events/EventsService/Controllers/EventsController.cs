using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace EventsService.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly string _kafkaBrokers;

    public EventsController()
    {
        _kafkaBrokers = Environment.GetEnvironmentVariable("KAFKA_BROKERS") ?? "localhost:9092";
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUserEvent([FromBody] UserEvent evt)
    {
        await PublishAndConsume("user-events", System.Text.Json.JsonSerializer.Serialize(evt));
       	return StatusCode(201, new { status = "success", type = "user", data = evt });
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePaymentEvent([FromBody] PaymentEvent evt)
    {
        await PublishAndConsume("payment-events", System.Text.Json.JsonSerializer.Serialize(evt));
        return StatusCode(201, new { status = "success", type = "payment", data = evt });
    }

    [HttpPost("movie")]
    public async Task<IActionResult> CreateMovieEvent([FromBody] MovieEvent evt)
    {
        await PublishAndConsume("movie-events", System.Text.Json.JsonSerializer.Serialize(evt));
        return StatusCode(201, new { status = "success", type = "movie", data = evt });
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = true });

    private async Task PublishAndConsume(string topic, string message)
    {
        var config = new ProducerConfig { BootstrapServers = _kafkaBrokers };

        using var producer = new ProducerBuilder<Null, string>(config).Build();
        await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
        Console.WriteLine($"[PRODUCER] topic={topic} message={message}");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaBrokers,
            GroupId = "events-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(topic);

        var result = consumer.Consume(TimeSpan.FromSeconds(5));
        if (result != null)
        {
            Console.WriteLine($"[CONSUMER] topic={topic} message={result.Message.Value}");
        }
        consumer.Unsubscribe();
    }
}