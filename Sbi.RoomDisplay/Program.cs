using Sbi.RoomDisplay.Services.Interfaces;
using Sbi.RoomDisplay.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Interface -> Implementation
builder.Services.AddHttpClient<IBookingService, ApiBookingService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Business logic builder
builder.Services.AddScoped<ScheduleBuilderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{code?}",
    defaults: new { controller = "Tv", action = "Room" });

app.Run();