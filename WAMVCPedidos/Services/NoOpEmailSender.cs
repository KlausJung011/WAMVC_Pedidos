using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WAMVCPedidos.Services
{
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
            => Task.CompletedTask;
    }
}