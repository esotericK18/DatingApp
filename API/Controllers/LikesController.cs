using System.Threading.Tasks;
using API.Entities;
using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using API.Helpers;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUserRepository _userRepository;
        public LikesController(IUserRepository userRepository, ILikeRepository likeRepository)
        {
            _userRepository = userRepository;
            _likeRepository = likeRepository;
        }

        [HttpPost("{likedUsername}")]
        public async Task<ActionResult> AddLike(string likedUsername)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepository.GetMemberByUserNameAsync(likedUsername);
            var sourceUser = await _likeRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();
            if (sourceUser.UserName == likedUsername) return BadRequest("You cannot like yourself.");

            var userLike = await _likeRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest("You already liked this user.");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _likeRepository.SaveAllAsync()) return Ok();

            return BadRequest("Like Failed");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likeRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}
