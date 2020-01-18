using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueTinder.Data;
using Microsoft.EntityFrameworkCore;
using RescueTinder.Models;

namespace RescueTinder.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext context;

        public UsersController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize]
        public IActionResult User(string id)
        {
            var user = context
                .Users
                .Include(u => u.Dogs)
                .Single(u => u.Id == id);

            var result = new UserViewModel
            {
                Name = user.FirstName + " " + user.LastName,
                BirthDate = user.BirthDate,
                Province = user.Province,
                PersonalNotes = user.PersonalNotes,
                ImageUrl = user.ImageUrl
            };

            foreach (var dog in user.Dogs)
            {
                var dogViewModel = new DogViewModel
                {
                    Id = dog.Id,
                    Name = dog.Name
                };

                foreach (var image in dog.Images)
                {
                    dogViewModel.Images.Add(image.Id, image.ImageUrl);
                }

                if (dog.Adopted)
                {
                    result.AdoptedDogs.Add(dogViewModel);
                }

                else
                {
                    result.Dogs.Add(dogViewModel);
                }
            }

            return View(result);
        }
    }
}