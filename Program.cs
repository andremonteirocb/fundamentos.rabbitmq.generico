using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue;
using Fundamentos.RabbitMQ.Generico.Extensions;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton((sp) => 
    new ConnectionFactory()
    {
        HostName = builder.Configuration["RabbitMqConfig:Host"]
    }
);

builder.Services.AddSingletonWithRetry<IConnection, BrokerUnreachableException>(sp => sp.GetRequiredService<ConnectionFactory>().CreateConnection());
builder.Services.AddTransient(sp => sp.GetRequiredService<IConnection>().CreateModel());

builder.Services.AddTransient<Publisher>();
builder.Services.AddTransient<Consumer>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var model = app.GetService<IModel>();
model.ExchangeDeclare(app.Configuration["RabbitMqConfig:FanoutExchange"], ExchangeType.Fanout, true);
model.QueueDeclare(app.Configuration["RabbitMqConfig:Queue"], true, false, false, null);
model.QueueBind(app.Configuration["RabbitMqConfig:Queue"], app.Configuration["RabbitMqConfig:FanoutExchange"], string.Empty);

app.GetService<Consumer>().QueueBind(app.Configuration["RabbitMqConfig:Queue"], 2);
//app.GetService<Consumer>().QueueBind("processar-nota-fiscal", 2);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();