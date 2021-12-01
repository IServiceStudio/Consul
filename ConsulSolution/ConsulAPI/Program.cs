using Consul;
using ConsulAPI.Options;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region 注入配置

builder.Services.Configure<ConsulOptions>(builder.Configuration.GetSection(ConsulOptions.Consul));
builder.Services.Configure<LocalServiceOptions>(builder.Configuration.GetSection(LocalServiceOptions.LocalService));

#endregion

#region 注入Consul

var consulOptions = builder.Configuration.GetSection(ConsulOptions.Consul).Get<ConsulOptions>();
var localServiceOptions = builder.Configuration.GetSection(LocalServiceOptions.LocalService).Get<LocalServiceOptions>();
ConsulClient consulClient = new ConsulClient(config =>
{
    config.Address = new Uri(consulOptions.Address);
    config.Datacenter = consulOptions.DataCenter;
});
consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
{
    ID = localServiceOptions.Name,
    Name = "ConsulService",
    Address = localServiceOptions.IP,
    Port = localServiceOptions.Port,
    Tags = new string[] { localServiceOptions.Weight },
    Check = new AgentServiceCheck()
    {
        Interval = TimeSpan.FromSeconds(consulOptions.HealthCheckInterval),
        HTTP = $"http://{localServiceOptions.IP}:{localServiceOptions.Port}/{consulOptions.HealthCheckAction}",
        Timeout = TimeSpan.FromSeconds(consulOptions.HealthCheckTimeOut),
        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(consulOptions.HealthCheckDeregisterCriticalServiceAfter)
    }
});

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/HealthCheck", () =>
{
    return "ok";
});

app.MapGet("/HelloConsul", async () =>
{
    using (var client = new ConsulClient(options =>
    {
        options.Address = new Uri("http://192.168.153.130:8500");
        options.Datacenter = "dc1";
    }))
    {
        var putPair = new KVPair("hello")
        {
            Value = Encoding.UTF8.GetBytes("Hello Consul")
        };

        var putAttempt = await client.KV.Put(putPair);

        if (putAttempt.Response)
        {
            var getPair = await client.KV.Get("hello");
            return Encoding.UTF8.GetString(getPair.Response.Value, 0,
                getPair.Response.Value.Length);
        }
        return "";
    }
});

#region 读取配置

//配置：读取所有配置
app.MapGet("/GetOptionsForAllConfig", (IConfiguration configRoot) =>
{
    var config = (IConfigurationRoot)configRoot;
    StringBuilder configStr = new StringBuilder();
    foreach (var provider in config.Providers.ToList())
    {
        configStr.AppendLine(provider.ToString());
    }
    return configStr.ToString();
});

//通过Config读取配置
app.MapGet("/GetOptionsByConfig", (IConfiguration configuration) =>
{
    return configuration["Consul:Address"];
});

//使用选项模式绑定分层配置数据:Bind
app.MapGet("/GetOptionsBySeparationForBind", (IConfiguration configuration) =>
{
    var consulOptions = new ConsulOptions();
    configuration.GetSection(ConsulOptions.Consul).Bind(consulOptions);
    return consulOptions;
});

//使用选项模式绑定分层配置数据:Get
app.MapGet("/GetOptionsBySeparationForSet", (IConfiguration configuration) =>
{
    var consulOptions = configuration.GetSection(ConsulOptions.Consul).Get<ConsulOptions>();
    return consulOptions;
});

app.MapGet("/GetOptions", (IOptions<ConsulOptions> options) =>
{
    var consulOptions = options.Value;
    return consulOptions;
});

#endregion

app.Run();