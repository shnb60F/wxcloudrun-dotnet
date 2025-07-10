using aspnetapp;
using aspnetapp.Middleware;
using aspnetapp.Service;
using aspnetapp.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.UseCors();
app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();
