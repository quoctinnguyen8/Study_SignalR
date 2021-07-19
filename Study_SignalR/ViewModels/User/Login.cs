using Study_SignalR.Common.CustomAttr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.ViewModels.User
{
	public class Login
	{
		private string username;
		[AppRequired]
		public string Username { get => username; set => username = value.Trim().ToLower(); }

		[AppRequired]
		public string Password { get; set; }
		public bool RememberMe { get; set; }
	}
}
