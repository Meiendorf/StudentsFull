using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using System.Diagnostics;

namespace Students.Data
{
    public class ScheduleMiddleware
    {
        private readonly RequestDelegate _next;

        public ScheduleMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext _db)
        {
            RecurringJob.AddOrUpdate<StudentJob>("studentJob", x => x.MakeStudent(), Cron.Minutely);
            await _next.Invoke(context);
        }

        
        public interface IStudentJob
        {
            Task MakeStudent();
        }

        public class StudentJob : IStudentJob
        {
            private ApplicationDbContext _db;
            public StudentJob(ApplicationDbContext _cont)
            {
                _db = _cont;
            }
            public async Task MakeStudent()
            {
                Debug.WriteLine("Adding student...");
                _db.Students.Add(new Models.Student()
                {
                    Name = "Schedule Student" + new Random().Next(),
                    GroupId = 1
                });
                await _db.SaveChangesAsync();
            }
        }

    }
}
