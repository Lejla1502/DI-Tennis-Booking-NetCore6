using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TennisBookings.DependencyInjection
{
	public static class BookingRulesServiceCollectionExtensions
	{
		//returns IServiceCollection which supports fluent chaning
		//we are extending IServiceCollection
		public static IServiceCollection AddBookingRules(this IServiceCollection services)
		{
			//registering multiple implementations of ICourtBookingInterface -->>>>>>>>> HERE WE USE "ADD" BECAUSE WE WANT ALL IMPLEMENTATIONS
			services.AddSingleton<ICourtBookingRule, ClubIsOpenRule>();
			services.AddSingleton<ICourtBookingRule, MaxBookingLengthRule>();
			services.AddSingleton<ICourtBookingRule, MaxPeakTimeBookingLengthRule>();
			services.AddScoped<ICourtBookingRule, MemberBookingsMustNotOverlapRule>(); //it depends on ICourtBookingService which is scoped
			services.AddScoped<ICourtBookingRule, MemberCourtBookingsMaxHoursPerDayRule>(); //it depends on ICourtBookingService which is scoped

			services.TryAddScoped<IBookingRuleProcessor, BookingRuleProcessor>();

			return services;
		}

	}
}
