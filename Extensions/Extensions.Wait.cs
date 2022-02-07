using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fundamentos.RabbitMQ.Generico.Core.Infrastructure
{
    public static partial class Extensions
    {
        public static void Wait(this TimeSpan time)        
        {
            System.Threading.Thread.Sleep(time);
        }
    }
}
