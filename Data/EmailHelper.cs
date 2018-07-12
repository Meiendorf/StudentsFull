using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Students.Data
{
    public static class EmailHelper
    {
        public static async Task<string> SendMailToAdmin(string msg, string subj, IConfiguration configuration)
        {
            var emailBody = new EmailBody()
            {
                subject = subj,
                content = msg,
                project = "6",
                to = configuration["adminMail"]
            };

            var client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://sns.esempla.com/sendMail");
            var jsonBody = JsonConvert.SerializeObject(emailBody);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.Headers.Add("Authorization", configuration["mailAuthHeader"]);

            HttpResponseMessage res;

            res = await client.SendAsync(request);

            return await res.Content.ReadAsStringAsync();
        }
    }

    public class EmailBody
    {
        public string to { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public string project { get; set; }
    }


}
