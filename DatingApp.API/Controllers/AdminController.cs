using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        public AdminController(DataContext dataContext, UserManager<User> userManager)
        {
            _userManager = userManager;
            _dataContext = dataContext;
        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpGet("userWithRoles")]
        public async Task<IActionResult> GetUserWithRoles()
        {
            var userList = await _dataContext.Users
                .OrderBy(x => x.UserName)
                .Select(x => new
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Roles = (from userRole in x.UserRoles
                                join role in _dataContext.Roles on userRole.RoleId
                                equals role.Id
                                select role.Name).ToList()
                }).ToListAsync();

            return Ok(userList);            
        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDTO roleEditDTO)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = roleEditDTO.RoleNames;
            selectedRoles = selectedRoles ?? new string[] {};

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (! result.Succeeded)
                return BadRequest("Failed to add the roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (! result.Succeeded)
                return BadRequest("Failed to remove the roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public IActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderatores can see this");
        }
    }
}