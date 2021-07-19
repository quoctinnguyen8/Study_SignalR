using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Study_SignalR.ChatHub.Data;
using Study_SignalR.Common;
using Study_SignalR.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Study_SignalR.ChatHub
{
	[Authorize]
	public class ChatHub : Hub
	{
		private readonly ConnectedUser AllUser;
		private readonly StudySignalRContext db;
		public ChatHub(ConnectedUser _lst, StudySignalRContext _db)
		{
			AllUser = _lst;
			db = _db;
		}

		public async Task SendMessage(int receiverId, string message)
		{
			try
			{
				var senderId = Convert.ToInt32(Context.UserIdentifier);
				var senderKey = Context.User.FindFirstValue("MessageKey");

				AppMessage mesg = new AppMessage
				{
					IsSeen = false,
					Message = message,
					ReceiverId = receiverId,
					SenderId = senderId,
					SendingTime = DateTime.Now
				};
				// Mã hóa tin nhắn trước khi lưu vào db, sử dụng key từ phía người gửi
				var encryptedMesg = AESThenHMAC.SimpleEncryptWithPassword(message, senderKey);
				mesg.Message = encryptedMesg;
				await db.AppMessages.AddAsync(mesg);
				await db.SaveChangesAsync();
				mesg.Message = message;
				await Clients
					.Users(new[] { receiverId.ToString(), Context.UserIdentifier })
					.SendAsync("ReceiveMessage", mesg);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		public async Task GetUserStateChange()
		{
			await Clients.All.SendAsync("ReceiveUserStateChange", AllUser.Ids);
		}

		public async Task SeenMessage(long mesgId, int senderId, int receiverId)
		{
			var mesg = await db.AppMessages
						.SingleOrDefaultAsync(x => x.Id == mesgId && x.SenderId == senderId && x.ReceiverId == receiverId);
			mesg.IsSeen = true;
			await db.SaveChangesAsync();
		}

		public override Task OnConnectedAsync()
		{
			int userId = Convert.ToInt32(Context.UserIdentifier);
			
			if(!AllUser.Ids.Any(s => s == userId))
			{
				AllUser.Ids.Add(userId);
			}
			_ = this.GetUserStateChange();
			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			int userId = Convert.ToInt32(Context.UserIdentifier);

			if (AllUser.Ids.Any(s => s == userId))
			{
				AllUser.Ids.Remove(userId);
			}
			_ = this.GetUserStateChange();
			return base.OnDisconnectedAsync(exception);
		}
	}
}
