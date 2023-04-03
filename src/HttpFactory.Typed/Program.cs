using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace HttpFactory.Typed;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var builder = new HostBuilder()
          .ConfigureServices((hostContext, services) =>
          {
              services.AddHttpClient<IMyService, MyService>(c => {
                  c.BaseAddress = new Uri("https://www.bbc.co.uk");
              });
            
          }).UseConsoleLifetime();

        var host = builder.Build();
        using var serviceScope = host.Services.CreateScope();

        var services = serviceScope.ServiceProvider;

        try
        {
            var myService = services.GetRequiredService<IMyService>();
            var pageContent = await myService.GetPage();
            Process currentProcess = Process.GetCurrentProcess();
            Console.WriteLine( currentProcess.Id);

            Console.WriteLine(pageContent.Substring(0, 500));
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
public interface IMyService
{
    Task<string> GetPage();
}

public class MyService : IMyService
{
    private readonly HttpClient _httpClient;

    public MyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetPage()
    {
        // Content from BBC One: Dr. Who website (©BBC)
        var request = new HttpRequestMessage(HttpMethod.Get,
            "/programmes/b006q2x0");
        
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            return $"StatusCode: {response.StatusCode}";
        }
    }
    public async Task<string> GetPageAsStream()
    {
        var request = new HttpRequestMessage(HttpMethod.Get,"/programmes/b006q2x0");

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync
                <string>(responseStream);
          
        }
        else
        {
            return $"StatusCode: {response.StatusCode}";
        }
    }
}
