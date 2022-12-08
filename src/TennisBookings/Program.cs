#region Global Usings
global using Microsoft.AspNetCore.Identity;

global using TennisBookings;
global using TennisBookings.Data;
global using TennisBookings.Domain;
global using TennisBookings.Extensions;
global using TennisBookings.Configuration;
global using TennisBookings.Caching;
global using TennisBookings.Shared.Weather;
global using TennisBookings.Services.Bookings;
global using TennisBookings.Services.Greetings;
global using TennisBookings.Services.Unavailability;
global using TennisBookings.Services.Bookings.Rules;
global using TennisBookings.Services.Notifications;
global using TennisBookings.Services.Time;
global using TennisBookings.Services.Staff;
global using TennisBookings.Services.Courts;
global using TennisBookings.Services.Security;
global using Microsoft.EntityFrameworkCore;
#endregion

using Microsoft.Data.Sqlite;
using TennisBookings.BackgroundService;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

//creating Service Descriptors manually

/*var sd1= new ServiceDescriptor(typeof(IWeatherForecaster), typeof(AmazingWeatherForcaster), ServiceLifetime.Transient);

var sd2 = ServiceDescriptor.Describe(typeof(IWeatherForecaster), typeof(AmazingWeatherForcaster), ServiceLifetime.Transient);

var sd3 = ServiceDescriptor.Transient(typeof(IWeatherForecaster), typeof(AmazingWeatherForcaster));

var sd4 = ServiceDescriptor.Transient<IWeatherForecaster, AmazingWeatherForcaster>();

*/

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICourtService, CourtService>();

builder.Services.AddScoped<ICourtBookingManager, CourtBookingManager>();
builder.Services.Configure<BookingConfiguration>(builder.Configuration.GetSection("CourtBookings"));
builder.Services.AddScoped<IBookingRuleProcessor, BookingRuleProcessor>();
builder.Services.AddSingleton<INotificationService, EmailNotificationService>();

builder.Services.AddScoped<ICourtBookingService, CourtBookingService>();
builder.Services.AddSingleton<IUtcTimeService, TimeService>();


builder.Services.AddTransient<IWeatherForecaster, AmazingWeatherForcaster>();

builder.Services.Configure<FeaturesConfiguration>(builder.Configuration.GetSection("Features"));

//registering multiple implementations of ICourtBookingInterface -->>>>>>>>> HERE WE USE "ADD" BECAUSE WE WANT ALL IMPLEMENTATIONS
builder.Services.AddSingleton<ICourtBookingRule, ClubIsOpenRule>();
builder.Services.AddSingleton<ICourtBookingRule, MaxBookingLengthRule>();
builder.Services.AddSingleton<ICourtBookingRule, MaxPeakTimeBookingLengthRule>();
builder.Services.AddScoped<ICourtBookingRule, MemberBookingsMustNotOverlapRule>(); //it depends on ICourtBookingService which is scoped
builder.Services.AddScoped<ICourtBookingRule, MemberCourtBookingsMaxHoursPerDayRule>(); //it depends on ICourtBookingService which is scoped

//and so that this can work, we also need to add configuration for rules

builder.Services.Configure<ClubConfiguration>(builder.Configuration.GetSection("ClubSettings"));




builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Bookings");
    options.Conventions.AuthorizePage("/BookCourt");
    options.Conventions.AuthorizePage("/FindAvailableCourts");
    options.Conventions.Add(new PageRouteTransformerConvention(new SlugifyParameterTransformer()));
});

#region InternalSetup
using var connection = new SqliteConnection("Filename=:memory:");
//using var connection = new SqliteConnection("Filename=test.db");
connection.Open();

// Add services to the container.
builder.Services.AddDbContext<TennisBookingsDbContext>(options => options.UseSqlite(connection));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<TennisBookingsUser, TennisBookingsRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<TennisBookingsDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddHostedService<InitialiseDatabaseService>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.AccessDeniedPath = "/identity/account/access-denied";
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
