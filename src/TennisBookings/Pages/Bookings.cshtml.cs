using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TennisBookings.Pages
{
	public class BookingsModel : PageModel
    {
        private readonly UserManager<TennisBookingsUser> _userManager;
        private readonly ICourtBookingService _courtBookingService;
		private readonly ILoggedInUserGreetingService _loggedInUserGreetingService;

		public BookingsModel(UserManager<TennisBookingsUser> userManager,
			ICourtBookingService courtBookingService,
			ILoggedInUserGreetingService loggedInUserGreetingService)
        {
            _userManager = userManager;
            _courtBookingService = courtBookingService;
			_loggedInUserGreetingService = loggedInUserGreetingService;
		}

        public IEnumerable<IGrouping<DateTime, CourtBooking>> CourtBookings { get; set; } = Array.Empty<IGrouping<DateTime, CourtBooking>>();

        public string Greeting { get; private set; } = "Hello";

        [TempData]
        public bool BookingSuccess { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (user == null)
                return new ChallengeResult();

            if (user.Member is not null)
			{
				Greeting = _loggedInUserGreetingService.GetLoggedInGreeting(user.Member.Forename);
				var bookings = await _courtBookingService.GetFutureBookingsForMemberAsync(user.Member);
                CourtBookings = bookings.GroupBy(x => x.StartDateTime.Date);
            }

            return Page();
        }
    }
}
