using Tripcare360.Application;
using Tripcare360.Infrastructure;
using Tripcare360.Infrastructure.Persistence;
using Tripcare360.WebApi;
using Tripcare360.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddWebApiServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<ResponseWrapperMiddleware>();
app.UseCors("AllowNextJs");
app.UseWhen(ctx => ctx.Request.Path.Value?.Contains("/sse/") != true,
    branch => branch.UseHttpsRedirection());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
