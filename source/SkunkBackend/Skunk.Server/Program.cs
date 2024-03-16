using Skunk.Server.DomainBus;
using Skunk.Server.Hubs;
using Skunk.Server.Json;
using Skunk.Server.Mongo;
using Skunk.Server.Reactive;
using Skunk.Server.Serial;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("Config/skunk.json",false)
    .AddJsonFile("Config/secrets.json", false);

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
builder.Services.AddHubs();
builder.Services.AddMongo(builder.Configuration);
builder.Services.AddDomainBus();
builder.Services.AddReactive();

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