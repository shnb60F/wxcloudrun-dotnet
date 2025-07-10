using aspnetapp;
using aspnetapp.Middleware;
using aspnetapp.Service;
using aspnetapp.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GuardersContext>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<GuardersService>();
builder.Services.AddSingleton<TreasureBoxService>();
builder.Services.AddSingleton<UserLoginService>();
builder.Services.AddSingleton<JWTUtil>();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseCors();
app.UseStaticFiles();
app.MapControllers();
app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();