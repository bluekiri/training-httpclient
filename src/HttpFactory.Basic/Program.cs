using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HttpFactory.Basic;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine($"Process: {System.Diagnostics.Process.GetCurrentProcess().Id}");
        Console.ReadKey();
        var builder = new HostBuilder()
         .ConfigureServices((hostContext, services) =>
         {
             services.AddHttpClient();
             services.AddTransient<IMyService, MyService>();
         }).UseConsoleLifetime();

        var host = builder.Build();
        using var serviceScope = host.Services.CreateScope();

        var services = serviceScope.ServiceProvider;

        try
        {
            var myService = services.GetRequiredService<IMyService>();
            var user = await myService.GetUser();

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
public record class UserDemo(int UserId, int Id, string Title, bool Completed);

public interface IMyService
{
    Task<UserDemo> GetUser();
    

}
public class MyService : IMyService
{
    private readonly IHttpClientFactory _clientFactory;

    private static  JsonSerializerOptions JsonSerializerOptions => new() { PropertyNameCaseInsensitive = true };
    
    public MyService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<UserDemo> GetUser()
    {
        
        // Content from BBC One: Dr. Who website (©BBC)
        var request = new HttpRequestMessage(HttpMethod.Get,
            "https://jsonplaceholder.typicode.com/todos/1");
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<UserDemo>(content,JsonSerializerOptions);
        }
        return null;
    }
}
