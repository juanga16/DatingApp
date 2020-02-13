using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfiguration;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository datingRepository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this._mapper = mapper;
            this._cloudinaryConfiguration = cloudinaryConfig;
            this._datingRepository = datingRepository;

            Account account = new Account(_cloudinaryConfiguration.Value.CloudName, _cloudinaryConfiguration.Value.ApiKey, _cloudinaryConfiguration.Value.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await _datingRepository.GetPhoto(id);
            var photoToReturn = _mapper.Map<PhotoForReturnDTO>(photo);

            return Ok(photoToReturn);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDTO photoForCreationDTO)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) 
                return Unauthorized();

            var user = await _datingRepository.GetUser(userId, true);
            var file = photoForCreationDTO.File;
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParameters = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParameters);                    
                }
            }

            photoForCreationDTO.Url = uploadResult.Uri.ToString();
            photoForCreationDTO.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDTO);

            if (!user.Photos.Any())
                photo.IsMain = true;
            
            user.Photos.Add(photo);            

            if (await _datingRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDTO>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId, Id = photo.Id }, photoToReturn);
            }

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id) 
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) 
                return Unauthorized();

            var user = await _datingRepository.GetUser(userId, true);

            if (!user.Photos.Any(x => x.Id == id))
                return Unauthorized();
            
            var photoToUpdate = await _datingRepository.GetPhoto(id);

            if (photoToUpdate.IsMain)
                return BadRequest("This is already the main photo");
            
            var currentMainPhoto = await _datingRepository.GetMainPhotoForUser(userId);

            currentMainPhoto.IsMain = false;
            photoToUpdate.IsMain = true;

            if (await _datingRepository.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) 
                return Unauthorized();

            var user = await _datingRepository.GetUser(userId, true);

            if (!user.Photos.Any(x => x.Id == id))
                return Unauthorized();
            
            var photoToDelete = await _datingRepository.GetPhoto(id);

            if (photoToDelete.IsMain)
                return BadRequest("You cannot delete your main photo");
            
            if (photoToDelete.PublicId != null)
            {
                var deleteParameters = new DeletionParams(photoToDelete.PublicId);
                var result = _cloudinary.Destroy(deleteParameters);

                if (result.Result == "ok")
                    _datingRepository.Delete(photoToDelete);            
            }
            
            if (photoToDelete.PublicId == null)
                _datingRepository.Delete(photoToDelete);            

            if (await _datingRepository.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}