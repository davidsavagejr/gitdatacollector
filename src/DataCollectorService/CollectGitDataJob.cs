using System;
using System.Diagnostics;
using System.Net.Http;
using Quartz;

namespace DataCollectorService
{
    public class CollectGitDataJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://api.github.com");
                    client.DefaultRequestHeaders.Add("User-Agent", "hsgitcollector");
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                    var repos = client.GetAsync("/search/repositories?q=*&sort=updated&order=asc").Result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
    }
}