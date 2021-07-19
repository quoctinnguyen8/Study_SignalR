using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.StartupConfig
{
	public static class RouterConfig
	{
		public static void MapAppRouter(IEndpointRouteBuilder endpoints)
		{
			endpoints.MapControllerRoute(
					name: "login",
					pattern: "login",
					defaults: new
					{
						controller = "User",
						action = "Login"
					});

			endpoints.MapControllerRoute(
					name: "registration",
					pattern: "reg",
					defaults: new
					{
						controller = "User",
						action = "Registration"
					});
			endpoints.MapControllerRoute(
					name: "chat",
					pattern: "chat-with-{id}",
					defaults: new
					{
						controller = "Home",
						action = "Index"
					});
		}
	}
}
