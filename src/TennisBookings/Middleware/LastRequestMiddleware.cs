namespace TennisBookings.Middleware
{
	public class LastRequestMiddleware 
	{
		private readonly RequestDelegate _next;
		private readonly IUtcTimeService _utcTimeService;

		public LastRequestMiddleware(RequestDelegate next,
			IUtcTimeService utcTimeService)
		{
			_next = next;
			_utcTimeService = utcTimeService;
		}

		public async Task InvokeAsync(HttpContext context, 
			UserManager<TennisBookingsUser> userManager)
		{
			if (context.User.Identity is not null &&
				context.User.Identity.IsAuthenticated)
			{
				var user = await userManager
					.FindByNameAsync(context.User.Identity.Name);

				if (user is not null)
				{
					user.LastRequestUtc = _utcTimeService.CurrentUtcDateTime;
					await userManager.UpdateAsync(user);
				}
			}

			await _next(context);
		}
	}

}
