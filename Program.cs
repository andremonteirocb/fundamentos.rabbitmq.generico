using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue;
using Fundamentos.RabbitMQ.Generico.Extensions;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure;

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

app.GetService<Consumer>().QueueBind("processar-pagamentos", 2);
//app.GetService<Consumer>().QueueBind("processar-nota-fiscal", 2);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
