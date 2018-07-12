using MassTransit;
using MessageContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Students.Services
{
    public class StudentConsumer : IConsumer<StudentContract>
    {
        public Task Consume(ConsumeContext<StudentContract> context)
        {
            Console.WriteLine("Student received " + context.Message.Name);
            return Task.CompletedTask;
        }
    }
}
