using MassTransit;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Students.Services
{
    public static class DefaultBus
    {
        public static IBusControl CreateBus(IConfigurationSection section)
        {
            
            return Bus.Factory.CreateUsingRabbitMq(c =>
            {
                var host = c.Host(new Uri(section["host"]), h =>
                {
                    h.Username(section["user"]);
                    h.Password(section["pass"]);
                });

                /*c.ReceiveEndpoint(host, "testQueue", e =>
                {

                    //e.PrefetchCount = 1;
                    e.Consumer<StudentConsumer>();
                });*/

            });
        }
    }
}
