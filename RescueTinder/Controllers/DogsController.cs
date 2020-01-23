using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RescueTinder.Data;
using RescueTinder.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;

namespace RescueTinder.Controllers
{
    public class DogsController : Controller
    {
        private ApplicationDbContext context;
        private UserManager<User> userManager;

        public DogsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Authorize]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> New(CreateDogViewModel model)
        {
            var validImage = model.Image == null || model.Image.FileName.EndsWith(".jpg");

            if (!ModelState.IsValid || !validImage || model.BirthDate > DateTime.UtcNow)
            {
                return View(model);
            }

            else
            {
                var user = await userManager.GetUserAsync(User);

                var dog = new Dog
                {
                    Name = model.Name,
                    BirthDate = model.BirthDate,
                    Province = model.Province,
                    Breed = model.Breed,
                    IsDisinfected = model.IsDisinfected,
                    IsVaccinated = model.IsVaccinated,
                    OwnerNotes = model.OwnerNotes,
                    IsCastrated = model.IsCastrated,
                    Gender = model.Gender,
                    Owner = user
                };

                if (model.Image != null)
                {

                    var acc = new CloudinaryDotNet.Account("dmm9z8uow", "367813196612582", "I3kSZZCbEN-OHiyD35eh8mzyO8k");

                    var cloud = new Cloudinary(acc);

                    var file = new FileDescription(model.Image.FileName, model.Image.OpenReadStream());

                    var upload = new ImageUploadParams()
                    {
                        File = file
                    };

                    var image = await cloud.UploadAsync(upload);

                    var pic = new Pic
                    {
                        ImageUrl = image.Uri.AbsoluteUri
                    };

                    dog.Images.Add(pic);
                }

                await context.Dogs.AddAsync(dog);

                await context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }


        }

        [Authorize]
        [HttpGet]
        public IActionResult Search()
        {
            return this.View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Search(SearchDogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dogs = new List<Dog>();

            dogs = context
                .Dogs
                .Include(d => d.Owner)
                .Include(d => d.Vet)
                .Include(d => d.VetNotes)
                .Include(d => d.Images)
                .ToList()
                .Where(d => DateTime.Now.Subtract(d.BirthDate).Days >= model.MinAge * 365 &&
                DateTime.Now.Subtract(d.BirthDate).Days <= model.MaxAge * 365 &&
                d.Province == model.Province &&
                d.Breed == model.Breed &&
                d.Gender == model.Gender &&
                d.Adopted == false)
                .ToList();

            if (model.IsVaccinated == true)
            {
                foreach (var dog in dogs.Where(d => d.IsVaccinated == false).ToList())
                {
                    dogs.Remove(dog);
                }
            }

            if (model.IsDisinfected == true)
            {
                foreach (var dog in dogs.Where(d => d.IsDisinfected == false).ToList())
                {
                    dogs.Remove(dog);
                }
            }

            if (model.IsCastrated == true)
            {
                foreach (var dog in dogs.Where(d => d.IsCastrated == false).ToList())
                {
                    dogs.Remove(dog);
                }
            }

            if (model.HasVetNotes == true)
            {
                foreach (var dog in dogs.Where(d => d.VetNotes.Count == 0).ToList())
                {
                    dogs.Remove(dog);
                }
            }

            var result = new List<DogViewModel>();

            foreach (var dog in dogs)
            {
                var dogModel = new DogViewModel
                {
                    Id = dog.Id,
                    Name = dog.Name,
                    Gender = dog.Gender,
                    BirthDate = dog.BirthDate,
                    Province = dog.Province,
                    IsVaccinated = dog.IsVaccinated,
                    IsDisinfected = dog.IsDisinfected,
                    IsCastrated = dog.IsCastrated,
                    Owner = dog.Owner.FirstName + " " + dog.Owner.LastName,
                    OwnerNotes = dog.OwnerNotes,
                    Vet = dog.Vet != null ? dog.Vet.FirstName + " " + dog.Vet.LastName : "No vet",
                    Breed = dog.Breed
                };

                foreach (var pic in dog.Images)
                {
                    dogModel.Images.Add(pic.Id, pic.ImageUrl);
                }

                result.Add(dogModel);
            }

            return View("SearchResults", result);

        }

        [Authorize]
        [HttpGet("/Dogs/Dog/{id?}")]
        public IActionResult Dog(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var dog = new Dog();


            dog = context.Dogs
                .Include(d => d.Owner)
                .Include(d => d.Vet)
                .Include(d => d.Images)
                .Include(d => d.VetNotes)
                .Include(d => d.LikedBy)
                .ThenInclude(l => l.User)
                .SingleOrDefault(d => d.Id == id);

            if (dog == null)
            {
                return RedirectToAction("Index", "Home");
            }


            var result = new DogViewModel
            {
                Id = dog.Id,
                Name = dog.Name,
                Gender = dog.Gender,
                BirthDate = dog.BirthDate,
                Province = dog.Province,
                IsVaccinated = dog.IsVaccinated,
                IsDisinfected = dog.IsDisinfected,
                IsCastrated = dog.IsCastrated,
                Adopted = dog.Adopted,
                Owner = dog.Owner.FirstName + " " + dog.Owner.LastName,
                OwnerNotes = dog.OwnerNotes,
                OwnerId = dog.Owner.Id,
                Vet = dog.Vet != null ? dog.Vet.FirstName + " " + dog.Vet.LastName : "No vet",
                VetId = dog.VetId,
                Breed = dog.Breed
            };

            foreach (var pic in dog.Images)
            {
                result.Images.Add(pic.Id, pic.ImageUrl);
            }

            foreach (var like in dog.LikedBy)
            {
                result.Likes.Add(new LikeViewModel
                {
                    Id = like.UserId + "tire" + like.DogId,
                    DogId = dog.Id,
                    UserId = like.UserId,
                    UserImageUrl = like.User.ImageUrl,
                    UserName = like.User.FirstName + " " + like.User.LastName
                });
            }

            foreach (var note in dog.VetNotes)
            {
                result.VetNotes.Add(note.CreatedOn, note.Content);
            }

            return View(result);
        }

        [Authorize]
        public async Task<IActionResult> Like(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var dog = context.Dogs.SingleOrDefault(d => d.Id == id);

            if (dog == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (dog.OwnerId == userManager.GetUserId(User))
            {
                return RedirectToAction("Dog", "Dogs", new { Id = id });
            }

            else
            {
                dog.LikedBy.Add(new Like
                {
                    Dog = dog,
                    UserId = userManager.GetUserId(HttpContext.User)
                });

                await context.SaveChangesAsync();

                return RedirectToAction("Dog", "Dogs", new { Id = id });
            }
        }

        [Authorize]
        public IActionResult My()
        {
            var dogs = new List<Dog>();

            using (context)
            {
                dogs = context.Dogs.Where(d => d.OwnerId == userManager.GetUserId(User))
                    .Include(d => d.Owner)
                    .Include(d => d.Vet)
                    .Include(d => d.Images)
                    .ToList();
            }

            var result = new List<DogViewModel>();

            foreach (var dog in dogs)
            {
                var dogModel = new DogViewModel
                {
                    Id = dog.Id,
                    Name = dog.Name,
                    Gender = dog.Gender,
                    BirthDate = dog.BirthDate,
                    Province = dog.Province,
                    IsVaccinated = dog.IsVaccinated,
                    IsDisinfected = dog.IsDisinfected,
                    IsCastrated = dog.IsCastrated,
                    Owner = dog.Owner.FirstName + " " + dog.Owner.LastName,
                    OwnerNotes = dog.OwnerNotes,
                    Vet = dog.Vet != null ? dog.Vet.FirstName + " " + dog.Vet.LastName : "No vet",
                    Breed = dog.Breed
                };

                foreach (var pic in dog.Images)
                {
                    dogModel.Images.Add(pic.Id, pic.ImageUrl);
                }

                result.Add(dogModel);
            }

            return View("SearchResults", result);
        }

        [Authorize]
        [HttpGet("Dogs/Edit/{id?}")]
        public IActionResult Edit(Guid id)
        {
            var dog = new Dog();


            dog = this.context.Dogs.Single(d => d.Id == id);

            if (dog.OwnerId != this.userManager.GetUserId(User))
            {
                return RedirectToAction("Index", "Home");
            }


            var result = new EditDogViewModel
            {
                Name = dog.Name,
                Gender = dog.Gender,
                BirthDate = dog.BirthDate,
                Province = dog.Province,
                IsVaccinated = dog.IsVaccinated,
                IsDisinfected = dog.IsDisinfected,
                IsCastrated = dog.IsCastrated,
                OwnerNotes = dog.OwnerNotes,
                Breed = dog.Breed
            };

            return View(result);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Edit(EditDogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            else
            {
                var dog = this.context.Dogs.Single(d => d.Id == model.Id);

                if (dog.OwnerId != this.userManager.GetUserId(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                dog.Name = model.Name;

                dog.Gender = model.Gender;

                dog.BirthDate = model.BirthDate;

                dog.Province = model.Province;

                dog.IsVaccinated = model.IsVaccinated;

                dog.IsDisinfected = model.IsDisinfected;

                dog.IsCastrated = model.IsCastrated;

                dog.OwnerNotes = model.OwnerNotes;

                dog.Breed = model.Breed;

                context.SaveChanges();

                return RedirectToAction("Dog", "Dogs", new { Id = model.Id });
            }
        }

        [Authorize]
        public IActionResult Adopt(string id)
        {
            var ids = id.Split("tire");

            var userId = ids[0];

            var dogId = Guid.Parse(ids[1]);

            var dog = this.context.Dogs.Single(d => d.Id == dogId);

            var user = this.context.Users.Single(u => u.Id == userId);

            if (dog.OwnerId != userManager.GetUserId(User))
            {
                return RedirectToAction("Index", "Home");
            }

            else
            {
                dog.Owner = user;

                dog.Adopted = true;

                context.SaveChanges();

                return RedirectToAction("About", "Messages", new { Id = id });
            }

        }

    }
}