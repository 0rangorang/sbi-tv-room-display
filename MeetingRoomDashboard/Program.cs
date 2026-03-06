using MeetingRoomDashboard.Services; // Supaya bisa akses service kita

var builder = WebApplication.CreateBuilder(args);

// Tambahkan MVC service ke container
builder.Services.AddControllersWithViews();

// =============================
// REGISTER DEPENDENCY INJECTION
// =============================
// Register service kita ke Dependency Injection container
// Artinya: setiap kali butuh IBookingService,
// ASP.NET akan memberikan FakeBookingService
builder.Services.AddScoped<IBookingService, FakeBookingService>();

// NOTE:
// Untuk production nanti, tinggal ganti menjadi:
// builder.Services.AddScoped<IBookingService, ApiBookingService>();
// Tanpa perlu ubah Controller

var app = builder.Build();

// =============================
// PIPELINE CONFIGURATION
// =============================
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
