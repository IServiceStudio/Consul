using Consul;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("GetConsul", () =>
{
    ConsulClient consulClient = new ConsulClient(config =>
    {
        config.Address = new Uri("http://192.168.153.133:8500");
        config.Datacenter = "dc1";
    });
    var consulResponse = consulClient.Agent.Services().Result.Response;
    foreach (var item in consulResponse)
    {
        var serviceKey = item.Key;
        var serviceValue = item.Value;
        var serviceUrl = $"{serviceValue.Address}--{serviceValue.Port}--{serviceValue.Service}";
    }
    return "ok";
});

app.Run();