using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HttpFactory.Named
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine($"Process: {System.Diagnostics.Process.GetCurrentProcess().Id}");
            Console.ReadKey();
            var builder = new HostBuilder()
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddHttpClient("myclient", c => {
                      c.BaseAddress = new Uri("https://www.bbc.co.uk");
                  });
                  services.AddHttpClient("myusersapi", c=> {
                      c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
                  });
                  services.AddTransient<IMyService, MyService>();
              }).UseConsoleLifetime();

            var host = builder.Build();
            using var serviceScope = host.Services.CreateScope();

            var services = serviceScope.ServiceProvider;

            try
            {
                var myService = services.GetRequiredService<IMyService>();
                 var pageContent = await myService.GetPage().ConfigureAwait(false);
                
                var userId = 1;
                if(args.Length >0 && int.TryParse(args[0], out int res)){
                    userId = res;
                }

                var user = await myService.GetUser(userId).ConfigureAwait(false);
                // await foreach( var result in  myService.GetUser2(userId) ){
                //     Console.WriteLine($"User: {JsonSerializer.Serialize(result)}");
                // }
                Console.WriteLine(pageContent.Substring(0, 500));
                Console.WriteLine($"User: {JsonSerializer.Serialize(user)}");
            }
            catch (Exception ex)
            {
   
                var logger = services.GetRequiredService<ILogger<Program>>();

                logger.LogError(ex, "An error occurred.");
            }
            Console.ReadLine();


            return 0;
        }
    }

    public class UserDemo
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
    }

    public interface IMyService
    {
        Task<string> GetPage();
        Task<UserDemo> GetUser(int id);
        IAsyncEnumerable<UserDemo> GetUser2(int id);
    }

    public class MyService : IMyService
    {
        private readonly IHttpClientFactory _clientFactory;

        private static  JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues =true };

        public MyService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<string> GetPage()
        {
            // Content from BBC One: Dr. Who website (©BBC)
            var request = new HttpRequestMessage(HttpMethod.Get,
                "/programmes/b006q2x0");
            var client = _clientFactory.CreateClient("myclient");
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"StatusCode: {response.StatusCode}";
            }
        }

        public async Task<UserDemo> GetUser(int id) 
        {

            var request = new HttpRequestMessage(HttpMethod.Get,$"/todos/{id}");
            var client = _clientFactory.CreateClient("myusersapi");
            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<UserDemo>(content,JsonSerializerOptions).ConfigureAwait(false);
            }
            return null;
        }
        public async IAsyncEnumerable<UserDemo> GetUser2(int id) 
        {
            for(var i = 0; i<10; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get,$"/todos/{id}");
                var client = _clientFactory.CreateClient("myusersapi");
                var response = await client.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    yield  return await JsonSerializer.DeserializeAsync<UserDemo>(content,JsonSerializerOptions).ConfigureAwait(false);
                }
                
            }
        }
    }










































}
