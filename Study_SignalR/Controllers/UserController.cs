using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Study_SignalR.Common;
using Study_SignalR.Entities;
using Study_SignalR.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Study_SignalR.Controllers
{
	public class UserController : AppControllerBase
	{
		private IHubContext<ChatHub.ChatHub> hubContext { get; set; }
		public UserController(StudySignalRContext _db, IHubContext<ChatHub.ChatHub> _hub) : base(_db)
		{
			hubContext = _hub;
		}

		public IActionResult Login()
		{
			if (User.Identity.IsAuthenticated)
			{
				return HomePage();
			}
			var model = new Login();
			if (TempData["User"] != null)
			{
				// TempData từ action Registration (POST)
				model = JsonConvert.DeserializeObject<Login>(TempData["User"].ToString());
				TempData["FromReg"] = "Đăng ký thành công, tiến hành đăng nhập ngay";
			}
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Login(Login model)
		{
			model.Password = model.Password.HashWith(model.Username);
			try
			{
				var user = await db.AppUsers.SingleOrDefaultAsync(m => m.Username == model.Username.ToLower() && m.Password == model.Password);
				if (user != null)
				{
					var claims = new List<Claim>{
						new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
						new Claim(ClaimTypes.Name, user.Username),
						new Claim(ClaimTypes.GivenName, user.FullName),
						new Claim("MessageKey",user.MessageKey)
					};
					var cookies = CookieAuthenticationDefaults.AuthenticationScheme;
					var claimsIdentity = new ClaimsIdentity(claims, cookies);
					var principal = new ClaimsPrincipal(claimsIdentity);
					var authProperties = new AuthenticationProperties()
					{
						IsPersistent = model.RememberMe,
						ExpiresUtc = DateTime.UtcNow.AddDays(7)
					};
					await HttpContext.SignInAsync(cookies, principal, authProperties);
					return HomePage();
				}
				TempData["Err"] = "Tên đăng nhập hoặc mật khẩu không hợp lệ";
				return RedirectToAction(nameof(Login));
			}
			catch { }

			TempData["Err"] = "Đã xảy ra lỗi khi thực hiện yêu cầu, vui lòng thử lại";
			return RedirectToAction(nameof(Login));
		}

		public IActionResult Registration()
		{
			if (User.Identity.IsAuthenticated)
			{
				return HomePage();
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Registration(Registration model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					if (await db.AppUsers.AnyAsync(m => m.Username == model.Username))
					{
						TempData["Err"] = "Tên đăng nhập đã tồn tại";
						return RedirectToAction(nameof(Registration));
					}
					string plainTextPwd = model.Password;
					model.Password = model.Password.HashWith(model.Username);
					model.ConfirmPassword = model.Password;

					var user = new AppUser
					{
						Username = model.Username,
						Password = model.Password,
						FullName = string.IsNullOrEmpty(model.FullName) ? model.Username : model.FullName,
						MessageKey = StringHasher.CreateSalt()
					};
					await db.AddAsync(user);
					await db.SaveChangesAsync();
					await hubContext.Clients.All.SendAsync("GetRegister", user.Id, user.Username, user.FullName);
					var loginData = new Login
					{
						Username = model.Username,
						Password = plainTextPwd,
						RememberMe = true
					};
					TempData["User"] = JsonConvert.SerializeObject(loginData);
					return RedirectToAction(nameof(Login));
				}
				catch { }
			}

			TempData["Err"] = "Dữ liệu không hợp lệ, vui lòng kiểm tra lại";
			return RedirectToAction(nameof(Registration));
		}

		[Authorize]
		public async Task<IActionResult> SignOut()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return HomePage();
		}
	}
}
