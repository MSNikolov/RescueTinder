using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTinder.Data;
using RescueTinder.Models;

namespace RescueTinder.Controllers
{
    public class LikesController : Controller
    {
        private ApplicationDbContext context;
        private UserManager<User> userManager;

        public LikesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public IActionResult All()
        {
            var likes = new List<Like>();

            using (context)
            {
                 likes = context.Likes
                    .Where(l => l.Dog.OwnerId == userManager.GetUserId(User) && l.Dog.Adopted == false)
                    .Include(l => l.Dog)
                    .Include(l => l.Dog.Images)
                    .Include(l => l.User)
                    .ToList();
            }

            var result = new List<LikeViewModel>();

            foreach (var like in likes)
            {
                var likeModel = new LikeViewModel
                {
                    Id = like.UserId+"tire"+like.DogId,
                    DogId = like.Dog.Id,
                    DogName = like.Dog.Name,
                    DogImageUrl = like.Dog.Images.First().ImageUrl,
                    UserId = like.User.Id,
                    UserName = like.User.FirstName + " " + like.User.LastName,
                    UserImageUrl = like.User.ImageUrl
                };

                result.Add(likeModel);
            }

            return View(result);
        }
    }
}