using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.Entities
{
	public class AppMessage
	{
		public AppMessage()
		{

		}
		public long Id { get; set; }
		public string Message { get; set; }
		public DateTime SendingTime { get; set; }
		public bool IsSeen { get; set; }
		public int SenderId { get; set; }
		public int ReceiverId { get; set; }

		public AppUser Sender { get; set; }
		public AppUser Receiver { get; set; }
	}
}
