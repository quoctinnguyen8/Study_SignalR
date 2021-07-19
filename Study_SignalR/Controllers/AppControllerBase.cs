using Microsoft.AspNetCore.Mvc;
using Study_SignalR.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.Controllers
{
	public class AppControllerBase : Controller
	{
		protected readonly StudySignalRContext db;

		public AppControllerBase(StudySignalRContext _db)
		{
			db = _db;
		}

		protected IActionResult HomePage()
		{
			return RedirectToAction("Index", "Home");
		}
	}
}
