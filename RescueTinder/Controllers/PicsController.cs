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

namespace RescueTinder.Controllers
{

    public class PicsController : Controller
    {

        private ApplicationDbContext context;
        private UserManager<User> userManager;

        public PicsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Add(Guid id)
        {
            var dog = this.context.Dogs.Single(d => d.Id == id);


            var model = new AddPicModel
            {
                Id = id
            };

            if (dog.OwnerId != this.userManager.GetUserId(User))
            {
                model = null;
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(AddPicModel model)
        {
            var dog = this.context.Dogs.Single(d => d.Id == model.Id);

            if (!model.Pic.FileName.EndsWith(".jpg") || dog.OwnerId != this.userManager.GetUserId(User))
            {
                return View(model);
            }

            else
            {
                var acc = new CloudinaryDotNet.Account("dmm9z8uow", "367813196612582", "I3kSZZCbEN-OHiyD35eh8mzyO8k");

                var cloud = new Cloudinary(acc);

                var file = new FileDescription(model.Pic.FileName, model.Pic.OpenReadStream());

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

                await context.SaveChangesAsync();

                return RedirectToAction("Dog", "Dogs", new { Id = model.Id });
            }
        }

        [Authorize]
        [HttpGet("/Pics/Show/{id?}")]
        public IActionResult Show(Guid id)
        {
            if (!ModelState.IsValid || !this.context.Pics.Any(p => p.Id == id))
            {
                return RedirectToAction("Index", "Home");
            }

            var pic  = context.Pics.Single(p => p.Id == id);

            var model = new ShowPicModel
            {
                Id = pic.Id,
                Url = pic.ImageUrl
            };

            return View(model);
        }
    }
}