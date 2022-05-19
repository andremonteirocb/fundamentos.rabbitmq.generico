namespace Fundamentos.RabbitMQ.Generico.Core.Infrastructure
{
    public static partial class Extensions
    {
        public static IService GetService<IService>(this WebApplication app)
            where IService : class
        {
            return ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IService>();
        }
    }
}
