using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RescueTinder.Data;
using RescueTinder.Models;

namespace RescueTinder.Controllers
{
    public class DogsController : Controller
    {
        private ApplicationDbContext context;
        private SignInManager<User> signInManager;
        private UserManager<User> userManager;

        public DogsController(ApplicationDbContext context, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            this.context = context;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> New(CreateDogViewModel model)
        {
            var validImage = model.Image.FileName.EndsWith(".jpg");

            if (!ModelState.IsValid || !signInManager.IsSignedIn(User) || !validImage)
            {
                return View(model);
            }

            else 
            {
                var dog = new Dog
                {
                    Name = model.Name,
                    BirthDate = model.BirthDate,
                    Province = model.Province,
                    Breed = model.Breed,
                    IsDisinfected = model.IsDisinfected,
                    IsVaccinated = model.IsVaccinated,
                    OwnerNotes = model.OwnerNotes,
                    Owner = await userManager.GetUserAsync(HttpContext.User)
                };

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

                using (context)
                {
                    await context.Dogs.AddAsync(dog);

                    await context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Home");
            }
        }
    }
}