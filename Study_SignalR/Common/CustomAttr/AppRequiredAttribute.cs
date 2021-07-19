using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.Common.CustomAttr
{
	public class AppRequiredAttribute : RequiredAttribute
	{
		public AppRequiredAttribute():base()
		{
			ErrorMessage = "Trường này là bắt buộc";
		}
	}
}
