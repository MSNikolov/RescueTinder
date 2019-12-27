using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueTinder.Data;
using RescueTinder.Models;

namespace RescueTinder.Controllers
{

    public class PicsController : Controller
    {

        private ApplicationDbContext context;

        public PicsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Add(Guid id)
        {
            var model = new AddPicModel
            {
                Id = id
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(AddPicModel model)
        {
            if (!model.Pic.FileName.EndsWith(".jpg"))
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

                var dog = new Dog();

                using (context)
                {
                    dog = context.Dogs.Single(d => d.Id == model.Id);

                    dog.Images.Add(pic);

                    await context.SaveChangesAsync();
                }

                return RedirectToAction("Dog", "Dogs", new { Id = model.Id });
            }
        }

        [Authorize]
        [HttpGet("/Pics/Show/{id?}")]
        public IActionResult Show (Guid id)
        {
            var pic = new Pic();

            using (context)
            {
                pic = context.Pics.Single(p => p.Id == id);
            }

            var model = new ShowPicModel
            {
                Id = pic.Id,
                Url = pic.ImageUrl
            };

            return View(model);
        }
    }
}