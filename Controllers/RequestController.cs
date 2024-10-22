﻿using System;
using Microsoft.AspNetCore.Mvc;
using PROJECTALTERAPI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PROJECTALTERAPI.Dtos;
using System.Security.Claims;
namespace PROJECTALTERAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly AlterDbContext _context;

        public RequestController(AlterDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("creatRequest")]
        public async Task<IActionResult> AddRequest(RequestDto requestDto)
        {
            var userId = GetCurrentUser();
            var request = new Request
            {
                UserId = userId.UserId,
                RequestTitle = requestDto.RequestTitle,
                RequestDescription = requestDto.RequestDescription,
                Deadline = requestDto.Deadline,
                RequestStatus = "NotTaken",
                RequestType = requestDto.RequestType
            };
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();
            return Ok(request);
        }
        [HttpGet("getRequests")]
        public async Task<IActionResult> GetRequests()
        {
            var requests = await _context.Requests.Where(r => r.RequestStatus == "NotTaken").ToListAsync();
            if (requests == null)
            {
                return NotFound();
            }
            return Ok(requests);
        }
        [HttpGet("getRequest")]
        public async Task<IActionResult> GetRequest()
        {
            var user = GetCurrentUser();
            var requests = await _context.Requests.Where(r => r.UserId == user.UserId && r.RequestStatus == "NotTaken").ToListAsync();
            if (requests == null)
            {
                return NotFound();
            }
            return Ok(requests);
        }
        [HttpGet("GetUserRequest/{requestId}")]
        public async Task<IActionResult> GetUserRequest(long requestId)
        {
            //var userId = GetCurrentUser();
            var request = await _context.Requests.Where(u => u.RequestId == requestId && u.RequestStatus == "NotTaken").FirstOrDefaultAsync();
            if (request == null)
            {
                return NotFound();
            }
            var user1 = await _context.Users.Where(u => u.UserId == request.UserId).FirstOrDefaultAsync();
            if (user1 == null)
            {
                return NotFound();
            }
            var userdto = new UserDto
            {
                UserId = user1.UserId,
                FirstName = user1.FirstName,
                LastName = user1.LastName,
                Username = user1.Username,
                Picture = user1.Picture,
            };
            return Ok(userdto);
        }
        [HttpGet("getUserRequests/{id}")]
        public async Task<IActionResult> GetUserRequests(long id)
        {
            var requests = await _context.Requests.Where(r => r.UserId == id && r.RequestStatus == "NotTaken").ToListAsync();
            if (requests == null)
            {
                return NotFound();
            }
            var requestDtos = requests.Select(r => new RequestDto
            {
                RequestId = r.RequestId,
                UserId = r.UserId,
                RequestTitle = r.RequestTitle,
                RequestDescription = r.RequestDescription,
                Deadline = r.Deadline
            }).ToList();
            return Ok(requestDtos);
        }
        [HttpGet("searchRequest")]
        public async Task<IActionResult> SearchRequest([FromQuery] string search)
        {
            var requests = await _context.Requests.Where(r => r.RequestTitle.Contains(search) && r.RequestStatus == "NotTaken").ToListAsync();
            if (requests == null)
            {
                return NotFound();
            }
            return Ok(requests);
        }
        [HttpGet("getRequestsBySkillType")]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequestsBySkillType([FromQuery] string skillType)
        {
            var requests = await _context.Requests.Where(r => r.RequestType == skillType).ToListAsync();
            if (requests == null)
            {
                return NotFound();
            }
            return Ok(requests);
        }
        [HttpPatch("uppdateRequest/{requestId}")]
        public async Task<IActionResult> UpdateRequest(long requestId, RequestDto requestDto)
        {
            var request = await _context.Requests.Where(r => r.RequestId == requestId).FirstOrDefaultAsync();
            if (request == null)
            {
                return NotFound();
            }
            request.RequestTitle = requestDto.RequestTitle;
            request.RequestDescription = requestDto.RequestDescription;
            request.Deadline = requestDto.Deadline;
            await _context.SaveChangesAsync();
            return Ok(request);
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
