var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddDapr();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Main");
app.UseRouting();
var i = 0;
app.MapGet("/healthx", async context =>
{
    context.Response.ContentType = "text/plain";
    context.Response.StatusCode = 200;
    logger.LogInformation("Request total {TotalCount}", Interlocked.Increment(ref i));
    await context.Response.WriteAsync($"api1 timestamp {DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds()}");
});
app.MapGet("/api/v1.0/persons", async context =>
{
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("""
                                      [
                                        {
                                          "name": "lewis zou",
                                          "age": 18  
                                        }
                                      ]
                                      """);
});
// app.UseHttpsRedirection();
app.UseCloudEvents();
// 配置终结点路由
app.MapSubscribeHandler(); // 注册订阅处理程序
app.MapDefaultControllerRoute();

app.Run();