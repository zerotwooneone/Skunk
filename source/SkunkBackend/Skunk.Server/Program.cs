using Skunk.Serial;
using Skunk.Server.Hubs;
using Skunk.Server.Json;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();



//todo:remove this test code
var serviceProvider = builder.Services.BuildServiceProvider();
var connection = new Connection(serviceProvider.GetRequiredService<ILogger<Connection>>());
var pLogger = serviceProvider.GetRequiredService<ILogger<Program>>();
connection.ReceivedString += (s, e) => {
    pLogger.LogInformation("string received {string}", e);
};
await connection.Open("COM5");



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