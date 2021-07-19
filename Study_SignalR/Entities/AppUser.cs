using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.Entities
{
	public class AppUser
	{
		public AppUser()
		{
			ReceivedMesg = new HashSet<AppMessage>();
			SendMesg = new HashSet<AppMessage>();
		}

		private string username;

		public int Id { get; set; }
		public string Username { get => username; set => username = value.Trim().ToLower();}
		public string Password { get; set; }
		public string FullName { get; set; }
		public string MessageKey { get; set; }

		public ICollection<AppMessage> ReceivedMesg { get; set; }
		public ICollection<AppMessage> SendMesg { get; set; }
	}
}
