using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.StartupConfig
{
	public static class LoginServiceExtension
	{
		public static AuthenticationBuilder AddLoginService(this IServiceCollection services)
		{
			return services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options => {
					options.ExpireTimeSpan = TimeSpan.FromDays(7);
					options.Cookie.HttpOnly = true;
					options.LoginPath = "/login";
				});
		}
	}
}
