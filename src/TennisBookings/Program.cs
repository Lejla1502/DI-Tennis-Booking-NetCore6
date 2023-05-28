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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TennisBookings.Services.Membership;
using TennisBookings.DependencyInjection;
using TennisBookings.Middleware;

var builder = WebApplication.CreateBuilder(args);



//creating Service Descriptors manually

/*var sd1= new ServiceDescriptor(typeof(IWeatherForecaster), typeof(AmazingWeatherForcaster), ServiceLifetime.Transient);

var sd2 = ServiceDescriptor.Describe(typeof(IWeatherForecaster), typeof(AmazingWeatherForcaster), ServiceLifetime.Transient);

var sd3 = ServiceDescriptor.Transient(typeof(IWeatherForecaster), typeof(AmazingWeatherForcaster));

var sd4 = ServiceDescriptor.Transient<IWeatherForecaster, AmazingWeatherForcaster>();

*/

builder.Services.TryAddScoped<IBookingService, BookingService>();
builder.Services.TryAddScoped<ICourtService, CourtService>();


builder.Services.TryAddScoped<ICourtBookingManager, CourtBookingManager>();
builder.Services.Configure<BookingConfiguration>(builder.Configuration.GetSection("CourtBookings"));
builder.Services.TryAddSingleton<INotificationService, EmailNotificationService>();

builder.Services.TryAddScoped<ICourtBookingService, CourtBookingService>();
builder.Services.TryAddSingleton<IUtcTimeService, TimeService>();

//when using Add method, RandomWeatherForecaster will be resolved from this, because it is last
//builder.Services.AddSingleton<IWeatherForecaster, AmazingWeatherForcaster>();
//builder.Services.AddSingleton<IWeatherForecaster, RandomWeatherForecaster>();

//when using TryAdd, AmazingWeatherForecaster will be resolved, because
//TryAdd will not register other service if there is already type registered for that service
builder.Services.TryAddSingleton<IWeatherForecaster, AmazingWeatherForcaster>();
builder.Services.TryAddSingleton<IWeatherForecaster, RandomWeatherForecaster>();

builder.Services.Configure<FeaturesConfiguration>(builder.Configuration.GetSection("Features"));

builder.Services.AddBookingRules();

//Forwarding registration by using implementation factory
builder.Services.TryAddSingleton<IBookingConfiguration>(sp =>
	sp.GetRequiredService<IOptions<BookingConfiguration>>().Value
	 );

//Implementation factory for when service provider can't automatically construct implementation
builder.Services.Configure<MembershipConfiguration>(builder.Configuration.GetSection("Membership"));
builder.Services.AddTransient<IMembershipAdvertBuilder, MembershipAdvertBuilder>();
//we use singleton since implementation of MembershipAdvert i immutable and therefore thread safe
builder.Services.AddSingleton<IMembershipAdvert>(sp =>
{
	var builder = sp.GetRequiredService<IMembershipAdvertBuilder>();
	builder.WithDiscount(10m);
	var advert = builder.Build();
	return advert;
});

//contains no mutable state, and constructing its' instance is expensive - it reads from json file
//leaving it like this will create two instances of GreetingService
//builder.Services.TryAddSingleton<IHomePageGreetingService, GreetingService>();
//builder.Services.TryAddSingleton<ILoggedInUserGreetingService, GreetingService>();
//BUT, to prevent it we can use 2 ways:
//1. - using overload of TryAddSingleton which accepts predefined instance of GreetingService
//	var greetingService = new GreetingService(builder.Environment);
//	builder.Services.TryAddSingleton<IHomePageGreetingService>(greetingService);
//	builder.Services.TryAddSingleton<ILoggedInUserGreetingService>(greetingService);
//downside -we are responsible for creating this instance
//plus this instance is not automatically disposed of or released for garbage collection
//2. - using implementation factories
//we are creating singleton registration of GreetingService so that we can access it from serviceProvider
builder.Services.TryAddSingleton<GreetingService>();
builder.Services.TryAddSingleton<IHomePageGreetingService>(sp =>
	sp.GetRequiredService<GreetingService>());
builder.Services.TryAddSingleton<ILoggedInUserGreetingService>(sp =>
	sp.GetRequiredService<GreetingService>());
//since now container is now responsible for creating GreetingService, any dependencies it requires,
//such as the IHostingEnvironment parameter can now be injected by the ServiceProvider at runtime
//With this change, the container owns the creation of instance, and if necessary,
//could dispose of it correctly

//Registering Open Generics
builder.Services.TryAddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));

//and so that this can work, we also need to add configuration for rules
builder.Services.Configure<ClubConfiguration>(builder.Configuration.GetSection("ClubSettings"));

//custom made rule
builder.Services.AddSingleton<ICourtBookingRule, DateTimeBookingMadeInFuture>();

builder.Services.AddScoped<ICourtMaintenanceService, CourtMaintenanceService>();


//builder.Services.AddScoped<IUnavailabilityProvider, ClubClosedUnavailabilityProvider>();
//builder.Services.AddScoped<IUnavailabilityProvider, CourtBookingUnavailabilityProvider>();
//builder.Services.AddScoped<IUnavailabilityProvider, OutsideCourtUnavailabilityProvider>();
//builder.Services.AddScoped<IUnavailabilityProvider, UpcomingHoursUnavailabilityProvider>();

//when registering multiple implementations of an interface, it is recommended to use
//TryAddEnumerable in case we have duplicates (because every single one would be registered, and
//sometimes it can lead to multiple side effects

//in order to implement TryAddEnumerable, we also need to work with service descriptors
builder.Services.TryAddEnumerable(new ServiceDescriptor[]
{
	ServiceDescriptor.Scoped<IUnavailabilityProvider, ClubClosedUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, ClubClosedUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, CourtBookingUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, OutsideCourtUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, UpcomingHoursUnavailabilityProvider>()
});
//here we are creating an array of service descriptors for services we need
//and even though we've registeres ClubClosed.. twice, it will only get resolved once




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

app.UseMiddleware<LastRequestMiddleware>();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
