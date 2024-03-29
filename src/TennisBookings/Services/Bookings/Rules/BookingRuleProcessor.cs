namespace TennisBookings.Services.Bookings.Rules
{
	public class BookingRuleProcessor : IBookingRuleProcessor
	{
		private readonly IEnumerable<ICourtBookingRule> _rules;

		public BookingRuleProcessor(IEnumerable<ICourtBookingRule> rules)
		{
			_rules = rules;
		}

		//it is possible to resolve multiple instances of an interface, because they are being passed
		//through IEnumerable
		public async Task<(bool, IEnumerable<string>)> PassesAllRulesAsync(CourtBooking courtBooking)
		{
			var passedRules = true;

			var errors = new List<string>();

			foreach (var rule in _rules)
			{
				if (!await rule.CompliesWithRuleAsync(courtBooking))
				{
					errors.Add(rule.ErrorMessage);
					passedRules = false;
				}
			}


			return (passedRules, errors);
		}
	}
}
