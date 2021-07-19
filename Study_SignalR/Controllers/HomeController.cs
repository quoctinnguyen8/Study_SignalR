using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Study_SignalR.Common;
using Study_SignalR.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Study_SignalR.Controllers
{
	public class HomeController : AppControllerBase
	{
		public HomeController(StudySignalRContext _db) : base(_db)
		{
		}

		public IActionResult Index(int? id)
		{
			if (!User.Identity.IsAuthenticated && id.HasValue)
			{
				return Redirect("/");
			}

			if (User.Identity.IsAuthenticated)
			{
				var listUser = db.AppUsers
								.Where(u => u.Username != User.Identity.Name)
								.OrderByDescending(u => u.Id)
								.ToHashSet();
				return View("Chat", listUser);
			}
			else
			{
				return View();
			}
		}

		[Authorize]
		public async Task<IActionResult> GetConversation(int partnerId, int? lastMesgId)
		{
			int myId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
			IEnumerable<AppMessage> mesgs;
			if (lastMesgId.HasValue)
			{
				mesgs = db.AppMessages.Where(m => m.Id < lastMesgId.Value && ((m.ReceiverId == partnerId && m.SenderId == myId)
									|| (m.ReceiverId == myId && m.SenderId == partnerId)))
					.OrderByDescending(m => m.Id)
					.Take(20);
			}
			else
			{
				mesgs = db.AppMessages.Where(m => (m.ReceiverId == partnerId && m.SenderId == myId)
									|| (m.ReceiverId == myId && m.SenderId == partnerId))
					.OrderByDescending(m => m.Id)
					.Take(20);
			}

			bool canGetMore = false;
			if (mesgs.Count() >= 20) canGetMore = true;

			// Giải mã tin nhắn
			var myKey = User.FindFirstValue("MessageKey");
			var partnerKey = (await (db.AppUsers
							.Select(x => new { x.MessageKey, x.Id }))
							.SingleOrDefaultAsync(u => u.Id == partnerId))
							.MessageKey;
			foreach (var item in mesgs)
			{
				var key = item.SenderId == myId ? myKey : partnerKey;
				item.Message = AESThenHMAC.SimpleDecryptWithPassword(item.Message, key);
			}
			return Ok(new { canGetMore, mesgs });
		}

		[Authorize]
		public async Task<IActionResult> GetUnseenMessage()
		{
			var myId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
			var userIds = await db.AppUsers
								.AsNoTracking()
								.Select(u => u.Id)
								.Where(u => u != myId)
								.ToListAsync();

			var lastestMessage = new HashSet<dynamic>();
			foreach (var id in userIds)
			{
				var lastMesg = await db.AppMessages
								.AsNoTracking()
								.Select(m => new { m.Id, m.IsSeen, m.ReceiverId, m.SenderId })
								.Where(m => m.ReceiverId == myId && id == m.SenderId)
								.OrderByDescending(m => m.Id)
								.Take(1)
								.FirstOrDefaultAsync();
				lastestMessage.Add(lastMesg);
			}
			return Ok(lastestMessage);
		}
	}
}
