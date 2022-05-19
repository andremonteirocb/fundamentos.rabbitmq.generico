using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue;
using Fundamentos.RabbitMQ.Generico.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    HostName = builder.Configuration["RabbitMqConfig:Host"],
    DispatchConsumersAsync = false,
    ConsumerDispatchConcurrency = 1,
    //UseBackgroundThreadsForIO = true
});

builder.Services.AddTransientWithRetry<IConnection, BrokerUnreachableException>(sp => sp.GetRequiredService<ConnectionFactory>().CreateConnection());
builder.Services.AddTransient(sp => sp.GetRequiredService<IConnection>().CreateModel());

builder.Services.AddTransient<Publisher>();
builder.Services.AddTransient<Consumer>();
builder.Services.BuildServiceProvider()
                .GetRequiredService<Consumer>()
                .Initialize("processar-pagamentos", 2);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
