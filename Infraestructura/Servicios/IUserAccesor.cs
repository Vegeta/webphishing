using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infraestructura.Identity;

namespace Infraestructura.Servicios {
	public interface IUserAccesor {
		public SessionInfo CurrentUser();
		public string? CurrentUsername();
		public int UserId();

		public string? IpAddress();
	}
}
