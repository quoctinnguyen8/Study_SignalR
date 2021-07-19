using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.ChatHub.Data
{

	public class ConnectedUser
	{
		public ConnectedUser()
		{
			Ids = new HashSet<int>();
		}
		public ICollection<int> Ids { get; set; }
	}
}
