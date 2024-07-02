using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PROJECTALTERAPI.Hubs;
namespace PROJECTALTERAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly AlterDbContext _db;
        public MessageController(AlterDbContext db, IHubContext<ChatHub> hubContext)
        {
            _db = db;

            _hubContext = hubContext;
        }
        [HttpPost("send/{id_receiver}")]
        public async Task<IActionResult> SendMessage(long id_receiver, [FromBody] MessageDto messageDto)
        {
            var user = GetCurrentUser();
            try
            {
                var message = new Message
                {
                    SenderId = user.UserId,
                    ReceiverId = id_receiver,
                    Content = messageDto.Content,
                    
                };

                _db.Messages.Add(message);
                await _db.SaveChangesAsync();

                var hubContext = (IHubContext<ChatHub>)HttpContext.RequestServices.GetService(typeof(IHubContext<ChatHub>));
                await hubContext.Clients.All.SendAsync("ReceiveMessage", new
                {
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    SenderName = user.Username,
                    ReceiverName = (await _db.Users.FindAsync(id_receiver)).Username,
                    Content = message.Content
                });

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllMessages/{id_receiver}")]
        public async Task<IActionResult> GetAllMessages(long id_receiver)
        {
            var user = GetCurrentUser();
            try
            {
                var messages = await _db.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Where(m => (m.SenderId == user.UserId && m.ReceiverId == id_receiver) || (m.SenderId == id_receiver && m.ReceiverId == user.UserId))
                    .OrderBy(m => m.MessageId) // Sort by time when message was created
                    .Select(m => new
                    {
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        SenderName = m.Sender.Username,
                        ReceiverName = m.Receiver.Username,
                        Content = m.Content
                    })
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private User GetCurrentUser()
        {
            var Identity = HttpContext.User.Identity as ClaimsIdentity;
            if (Identity != null)
            {
                var userClaim = Identity.Claims;
                return new User
                {
                    UserId = Convert.ToInt64(userClaim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value),
                    Username = userClaim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty
                };
            }
            return null!; // Add a return statement for the case when Identity is null
        }
    }
}

