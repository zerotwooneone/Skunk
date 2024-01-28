using Microsoft.Extensions.Options;
using Skunk.Serial;
using Skunk.Serial.Configuration;
using Skunk.Server.Hubs;
using Skunk.Server.Json;
using Skunk.Server.Serial;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("skunk.json");

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => { 
        //insert ours at the begining of the chain
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0,ServerJsonContext.Default); 
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddSingleton<ServerJsonContext>();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

builder.Services.AddSerial(builder.Configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    //this must be AFTER UseRouting, but BEFORE UseAuthorization
    app.UseCors();
}

app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");
app.MapHub<FrontendHub>("/FrontendHub");
app.Run();