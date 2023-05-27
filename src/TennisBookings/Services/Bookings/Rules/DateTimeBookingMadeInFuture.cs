namespace TennisBookings.Services.Bookings.Rules
{
	public class DateTimeBookingMadeInFuture : ICourtBookingRule
	{
		//ensure bookings are only valid if they are made for a date ant time which are placed in the future 
		private readonly IUtcTimeService _utcService;

		public DateTimeBookingMadeInFuture(IUtcTimeService utcService)
		{
			_utcService = utcService;
		}
		public string ErrorMessage => "Not possible to book a court for chosen time";

		public Task<bool> CompliesWithRuleAsync(CourtBooking booking)
		{
			if (booking.StartDateTime > _utcService.CurrentUtcDateTime)
				return Task.FromResult(true);
			else
				return Task.FromResult(false);

			//return Task.FromResult( _utcService.CurrentUtcDateTime < booking.StartDateTime);
		}

	}
}
