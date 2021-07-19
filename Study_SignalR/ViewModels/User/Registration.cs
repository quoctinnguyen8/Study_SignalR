using Study_SignalR.Common.CustomAttr;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.ViewModels.User
{
	public class Registration
	{
		private string username;
		[AppRequired]
		public string Username { get => username; set => username = value.Trim().ToLower(); }

		[AppRequired]
		[MinLength(5, ErrorMessage = "Mật khẩu quá ngắn")]
		public string Password { get; set; }

		[Compare(nameof(Password), ErrorMessage = "Mật khẩu không khớp")]
		public string ConfirmPassword { get; set; }
		public string FullName { get; set; }
	}
}
