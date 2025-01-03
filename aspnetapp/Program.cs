using aspnetapp;
using aspnetapp.Entity;
using aspnetapp.Middleware;
using aspnetapp.Service;
using aspnetapp.Utils;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "aspnetapp", Version = "v1" });
});
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
builder.Services.AddScoped<GuardersService>();
builder.Services.AddScoped<JWTUtil>();
builder.Services.AddScoped<GuardersSessionManager>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "aspnetapp v1");
        c.RoutePrefix = string.Empty; // 设置Swagger UI的根路径
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.UseMiddleware<RequestLoggingMiddleware>();

GuardersSession guardersSession0 = new GuardersSession();
Archer entity0 = new Archer { HP = 100 };
Archer entity1 = new Archer { HP = 100 };
guardersSession0.AddEntity("0", entity1);
guardersSession0.AddEntity("1", entity1);

GuardersSessionManager guardersSessionManager = app.Services.GetRequiredService<GuardersSessionManager>();
guardersSessionManager.AddSession(("Wechat0", "User0"));
guardersSessionManager.GetSession(("Wechat0", "User0")).ReadJson(@"
    {
        ""entitys"": {
            ""0"": {
                ""Type"": ""Archer"",
                ""HP"": 100
            },
            ""1"": {
                ""Type"": ""Archer"",
                ""HP"": 100
            }
        }
    }");

bool result = guardersSession0.CompareEntities(guardersSessionManager.GetSession(("Wechat0", "User0")));

app.Run();