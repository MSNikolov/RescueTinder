using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTinder.Data;
using RescueTinder.Models;

namespace RescueTinder.Controllers
{
    public class VetsController : Controller
    {
        private UserManager<User> userManager;
        private ApplicationDbContext context;
        private RoleManager<IdentityRole> roleManager;

        public VetsController(UserManager<User> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.context = context;
            this.roleManager = roleManager;
        }

        [Authorize]
        [HttpGet]
        public IActionResult New()
        {
            if (User.IsInRole("Vet"))
            {
                return RedirectToAction("Index", "Home");
            }

            else
            {
                return View();
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult New(string vetLicence)
        {
            if (User.IsInRole("Vet") || !ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            else
            {
                var user = this.context.Users.Single(u => u.Id == this.userManager.GetUserId(User));

                user.VetLicence = vetLicence;

                user.VetAprovedByAdmin = false;

                context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Manage()
        {
            var vets = this.context.Users.Where(u => u.VetLicence != null).ToList();

            var result = new List<VetViewModel>();

            foreach (var vet in vets)
            {
                result.Add(new VetViewModel
                {
                    Id = vet.Id,
                    ImageUrl = vet.ImageUrl,
                    Name = vet.FirstName + " " + vet.LastName,
                    Email = vet.Email,
                    BirthDate = vet.BirthDate,
                    VetLicence = vet.VetLicence,
                    AprovedByAdmin = vet.VetAprovedByAdmin
                });
            }

            return View(result);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Aprove(string id)
        {
            var user = this.context.Users.Single(u => u.Id == id);

            user.VetAprovedByAdmin = true;

            await userManager.AddToRoleAsync(user, "Vet");

            await context.SaveChangesAsync();

            return RedirectToAction("Manage", "Vets");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Stop(string id)
        {
            var user = this.context.Users.Single(u => u.Id == id);

            user.VetAprovedByAdmin = false;

            await userManager.RemoveFromRoleAsync(user, "Vet");

            await context.SaveChangesAsync();

            return RedirectToAction("Manage", "Vets");
        }

        [Authorize]
        public IActionResult Choose(Guid id)
        {
            var dog = this.context.Dogs.Single(d => d.Id == id);

            if (dog.OwnerId != this.userManager.GetUserId(User) || dog.Vet != null)
            {
                return RedirectToAction("Index", "Home");
            }

            var vets = this.context.Users.Where(u => u.VetAprovedByAdmin).ToList();

            var result = new List<DogChooseVetViewModel>();

            foreach (var vet in vets)
            {
                result.Add(new DogChooseVetViewModel
                {
                    DogId = dog.Id,
                    VetId = vet.Id,
                    VetImageUrl = vet.ImageUrl,
                    VetName = vet.FirstName + " " + vet.LastName
                });
            }

            return View(result);
        }

        [Authorize]
        public IActionResult Select(string id)
        {
            var ids = id.Split("tire");

            var dogId = Guid.Parse(ids[0]);

            var vetId = ids[1];

            var dog = this.context.Dogs.SingleOrDefault(d => d.Id == dogId);

            var vet = this.context.Users.SingleOrDefault(u => u.Id == vetId);

            if (userManager.GetUserId(User) != dog.OwnerId || dog.Vet != null || dog == null || vet == null || vet.VetAprovedByAdmin == false)
            {
                return RedirectToAction("Index", "Home");
            }

            dog.Vet = vet;

            context.SaveChanges();

            return RedirectToAction("Dog", "Dogs", new { Id = dogId });
        }

        [Authorize(Roles = "Vet")]
        public IActionResult Patients()
        {
            var patients = this.context
                        .Dogs
                        .Where(d => d.VetId == userManager.GetUserId(User))
                        .Include(d => d.Owner)
                        .Include(d => d.Vet)
                        .Include(d => d.Images)
                        .ToList();

            var result = new List<DogViewModel>();

            foreach (var dog in patients)
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

            return View(result);
        }

        [Authorize(Roles = "Vet")]
        public IActionResult Edit(Guid id)
        {
            var dog = this.context.Dogs.Single(d => d.Id == id);

            if (dog.VetId == userManager.GetUserId(User))
            {
                var model = new VetNoteViewModel
                {
                    IsVaccinated = dog.IsVaccinated,
                    IsDisinfected = dog.IsDisinfected,
                    IsCastrated = dog.IsCastrated
                };

                return View(model);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Vet")]
        [HttpPost]
        public IActionResult Edit(VetNoteViewModel model)
        {
            var dog = this.context.Dogs.Single(d => d.Id == model.Id);

            var vet = new User();

            if (dog.VetId != this.userManager.GetUserId(User))
            {
                return RedirectToAction("Index", "Home");
            }

            vet = this.context.Users.Single(u => u.Id == this.userManager.GetUserId(User));

            dog.IsVaccinated = model.IsVaccinated;

            dog.IsDisinfected = model.IsDisinfected;

            dog.IsCastrated = model.IsCastrated;

            if (model.VetNote != null)
            {
                dog.VetNotes.Add(new VetNote
                {
                    Subject = dog,
                    Vet = vet,
                    CreatedOn = DateTime.UtcNow,
                    Content = model.VetNote
                });
            }

            context.SaveChanges();

            return RedirectToAction("Dog", "Dogs", new { Id = model.Id });
        }
    }
}