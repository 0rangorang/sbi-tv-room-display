// ===============================
// USING (HARUS PALING ATAS)
// ===============================
using Sbi.RoomDisplay.Services.Interfaces;
using Sbi.RoomDisplay.Services.Implementations;


// ===============================
// APP BUILDER
// ===============================
var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();


// ===============================
// DEPENDENCY INJECTION
// ===============================

// Interface → Implementation
//builder.Services.AddScoped<IBookingService, MockBookingService>();
builder.Services.AddHttpClient<IBookingService, ApiBookingService>();


// Business logic builder
builder.Services.AddScoped<ScheduleBuilderService>();


var app = builder.Build();



// ===============================
// MIDDLEWARE PIPELINE
// ===============================
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
