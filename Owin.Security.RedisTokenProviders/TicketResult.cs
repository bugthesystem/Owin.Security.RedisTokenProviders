using Microsoft.Owin.Security;

namespace Owin.Security.RedisTokenProviders
{
    internal class TicketResult
    {
        public bool Deleted { get; set; }
        public AuthenticationTicket Ticket { get; set; }
    }
}