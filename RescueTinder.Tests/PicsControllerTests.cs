using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using Moq;
using NUnit.Framework;
using RescueTinder.Controllers;
using RescueTinder.Data;
using RescueTinder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace RescueTinder.Tests
{
    public class PicsControllerTests
    {
        private PicsController picsController;
        private ApplicationDbContext context;
        private UserManager<User> userManager;
        private CreateDogViewModel createDog;

        [SetUp]
        public void CtorShouldWork()
        {
            var dog = new CreateDogViewModel
            {
                Name = "Pesho",
                BirthDate = DateTime.UtcNow,
                Province = Province.Burgas,
                IsDisinfected = true,
                IsVaccinated = false,
                OwnerNotes = "BlaBla",
                Breed = DogBreed.JackRussellTerrier
            };

            this.createDog = dog;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ResqueDb")
                .Options;


            var db = new ApplicationDbContext(options);

            this.context = db;

            var store = new Mock<IUserStore<User>>();

            var userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);


            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, "abv@abv.bg")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");

            var claimsPrincipal = new ClaimsPrincipal(identity);

            var fakeUser = new User
            {
                Email = "abv@abv.bg",
                FirstName = "Pesho",
                LastName = "Gosho",
                ImageUrl = "userpic",
                Province = Province.Blagoevgrad,
                BirthDate = DateTime.UtcNow
            };

            this.context.Users.Add(fakeUser);

            this.context.SaveChanges();

            userManager.Setup(u => u.GetUserAsync(claimsPrincipal)).ReturnsAsync(this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg"));
            userManager.Setup(u => u.GetUserId(claimsPrincipal)).Returns(this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id);

            this.userManager = userManager.Object;

            var picsController = new PicsController(this.context, this.userManager);

            picsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            this.picsController = picsController;
        }

        [Test]
        public void AddPicShouldReturnTheCorrectDog()
        {
            var doggo = new Dog
            {
                Name = "Stasich",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(doggo);

            this.context.SaveChanges();

            var id = doggo.Id;

            var result = this.picsController.Add(id) as ViewResult;

            Assert.IsAssignableFrom<AddPicModel>(result.Model);

            var model = result.Model as AddPicModel;

            Assert.AreEqual(id, model.Id);
        }

        [Test]
        public void ShouldNotBeAbleToSeeAddPicViewIfNotOwner()
        {
            var doggy = new Dog
            {
                Name = "Doggy"
            };

            this.context.Dogs.Add(doggy);

            this.context.SaveChanges();

            var result = this.picsController.Add(doggy.Id) as RedirectToActionResult;

            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void UploadingAPicShouldNotWorkWithOtherThanDotJpg()
        {
            var doggo = new Dog
            {
                Name = "Visilka",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(doggo);

            this.context.SaveChanges();

            using (var stream = File.OpenRead(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpeg"))
            {
                var pic = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpeg"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };

                var model = new AddPicModel
                {
                    Id = doggo.Id,
                    Pic = pic
                };

                this.picsController.Add(model);

                Assert.AreEqual(0, doggo.Images.Count);
            }
        }

        [Test]
        public void AddPicShouldAddAPicture()
        {

            var doggo = new Dog
            {
                Name = "Genadi",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(doggo);

            this.context.SaveChanges();

            using (var stream = File.OpenRead(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpg"))
            {
                var pic = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpg"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpg"
                };

                var model = new AddPicModel
                {
                    Id = doggo.Id,
                    Pic = pic
                };

                var result = this.picsController.Add(model).Result as RedirectToActionResult;

                Assert.AreEqual("Dog", result.ActionName);

                Assert.AreEqual(1, doggo.Images.Count);
            }
        }

        [Test]
        public void ShouldNotBeAbleToAddPicIfNoOwner()
        {
            var fakeUser = new User
            {
                Email = "debil@faker.bg"
            };

            this.context.Users.Add(fakeUser);

            this.context.SaveChanges();

            var doggo = new Dog
            {
                Name = "Debil",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "debil@faker.bg").Id
            };

            this.context.Dogs.Add(doggo);

            this.context.SaveChanges();

            using (var stream = File.OpenRead(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpg"))
            {
                var pic = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpg"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpg"
                };

                var model = new AddPicModel
                {
                    Id = doggo.Id,
                    Pic = pic
                };

                var result = this.picsController.Add(model).Result as RedirectToActionResult;

                Assert.AreEqual(0, doggo.Images.Count);
            }
        }
        [Test]
        public void ShowPicShouldReturnCorrectImage()
        {
            var doggo = new Dog
            {
                Name = "Genadi",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(doggo);

            this.context.SaveChanges();

            using (var stream = File.OpenRead(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpg"))
            {
                var pic = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(@"C:\Users\Miroslav\source\repos\RescueTinder\RescueTinder\Data\Files\Maconi.jpg"))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpg"
                };

                var model = new AddPicModel
                {
                    Id = doggo.Id,
                    Pic = pic
                };

                var result = this.picsController.Add(model).Result as RedirectToActionResult;
            }

            Assert.AreEqual(1, doggo.Images.Count);

            var picture = doggo.Images.First();

            var showResult = this.picsController.Show(picture.Id) as ViewResult;

            Assert.IsAssignableFrom<ShowPicModel>(showResult.Model);

            var resultModel = showResult.Model as ShowPicModel;

            Assert.AreEqual(resultModel.Url, picture.ImageUrl);

        }
    }
}
