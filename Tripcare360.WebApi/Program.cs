using Tripcare360.Application;
using Tripcare360.Infrastructure;
using Tripcare360.WebApi;
using Tripcare360.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddWebApiServices();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<ResponseWrapperMiddleware>();
app.UseCors("AllowNextJs");
app.UseWhen(ctx => ctx.Request.Path.Value?.Contains("/sse/") != true,
    branch => branch.UseHttpsRedirection());
app.MapControllers();

app.Run();
