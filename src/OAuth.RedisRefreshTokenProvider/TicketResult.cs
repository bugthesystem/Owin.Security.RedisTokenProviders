using Microsoft.Owin.Security;

namespace OAuth.RedisRefreshTokenProvider
{
    internal class TicketResult
    {
        public bool Deleted { get; set; }
        public AuthenticationTicket Ticket { get; set; }
    }
}