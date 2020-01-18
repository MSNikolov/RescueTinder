using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using System.Threading.Tasks;

namespace RescueTinder.Tests
{
    [TestFixture]
    public class LikesControllerTests
    {
        private LikesController likesController;
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
                Province = Province.Blagoevgrad,
                BirthDate = DateTime.UtcNow
            };

            this.context.Users.Add(fakeUser);

            this.context.SaveChanges();

            userManager.Setup(u => u.GetUserAsync(claimsPrincipal)).ReturnsAsync(this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg"));
            userManager.Setup(u => u.GetUserId(claimsPrincipal)).Returns(this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id);

            this.userManager = userManager.Object;

            var likesController = new LikesController(this.context, this.userManager);

            likesController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            this.likesController = likesController;
        }

        [Test]
        public void AllLikesShouldWorkCorrectly()
        {
            var user = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg");

            var dog = new Dog
            {
                Name = "Blago",
                OwnerId = user.Id
            };

            dog.Images.Add(new Pic
            {
                Dog = dog,
                ImageUrl = "dogodog"
            });

            this.context.Dogs.Add(dog);
            

            this.context.SaveChanges();

            var liker = new User
            {
                Email = "master@abv.bg",
                FirstName = "Milko",
                LastName = "Kraev",
                ImageUrl = "milkokraev"
            };

            this.context.Users.Add(liker);

            this.context.SaveChanges();

            this.context.Dogs.Single(d => d.Name == "Blago")
                .LikedBy.Add(new Like
                {
                    DogId = this.context.Dogs.Single(d => d.Name == "Blago").Id,
                    UserId = this.context.Users.Single(u => u.Email == "master@abv.bg").Id
                });

            this.context.SaveChanges();

            var likes = this.likesController.All() as ViewResult;

            var result = likes.Model as List<LikeViewModel>;

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("Milko Kraev", result.First().UserName);

            Assert.AreEqual("Blago", result.First().DogName);

            var newFaker = new User
            {
                Email = "fakerfaker@fakemail.com",
                FirstName = "Bobi",
                LastName = "Dimitrov",
                ImageUrl = "fakerfaker"
            };

            this.context.Users.Add(newFaker);

            this.context.SaveChanges();

            dog.LikedBy.Add(new Like
            {
                DogId = dog.Id,
                UserId = newFaker.Id
            });

            this.context.SaveChanges();

            var resultTwo = this.likesController.All() as ViewResult;

            Assert.IsAssignableFrom<List<LikeViewModel>>(resultTwo.Model);

            var resultTwoAsList = resultTwo.Model as List<LikeViewModel>;

            Assert.AreEqual(2, resultTwoAsList.Count);

            Assert.IsTrue(resultTwoAsList.Any(r => r.DogName == "Blago" && r.UserName == "Bobi Dimitrov"));

            var fakeDog = new Dog
            {
                Name = "Drago",
                OwnerId = newFaker.Id
            };

            this.context.Dogs.Add(fakeDog);

            this.context.SaveChanges();

            fakeDog.LikedBy.Add(new Like
            {
                Dog = fakeDog,
                User = liker
            });

            this.context.SaveChanges();

            var resultThree = this.likesController.All() as ViewResult;

            var resultModel = resultThree.Model as List<LikeViewModel>;

            Assert.AreEqual(2, resultModel.Count);
        }
    }
}
