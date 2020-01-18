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
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace RescueTinder.Tests
{
    [TestFixture]
    public class UserControllerTests
    {
        private UsersController usersController;
        private ApplicationDbContext context;
        private UserManager<User> userManager;

        [SetUp]
        public void CtorShouldWork()
        {

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

            var usersController = new UsersController(this.context);

            usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            this.usersController = usersController;
        }

        [Test]
        public void ShowUserShouldShowCorrectUserData()
        {
            var fakeUser = new User
            {
                Email = "manfredmann@abv.bg",
                FirstName = "Manfred",
                LastName = "Mann",
                BirthDate = DateTime.Parse("24.11.1990"),
                Province = Province.Vratsa,
                ImageUrl = "imageurl"
            };

            this.context.Users.Add(fakeUser);

            this.context.SaveChanges();

            var firstFakeDog = new Dog
            {
                Name = "Gligan",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "manfredmann@abv.bg").Id
            };

            firstFakeDog.Images.Add(new Pic
            {
                ImageUrl = "fakeimageurl"
            });

            this.context.Dogs.Add(firstFakeDog);

            this.context.SaveChanges();

            var secondFakeDog = new Dog
            {
                Name = "Glistan",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "manfredmann@abv.bg").Id
            };

            secondFakeDog.Images.Add(new Pic
            {
                ImageUrl = "fakeimageurlnumbertwo"
            });

            this.context.Dogs.Add(secondFakeDog);

            this.context.SaveChanges();

            var thirdFakeDog = new Dog
            {
                Name = "NoMoreHomeless",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "manfredmann@abv.bg").Id,
                Adopted = true
            };

            thirdFakeDog.Images.Add(new Pic
            {
                ImageUrl = "fakeimageurlthree"
            });

            this.context.Dogs.Add(thirdFakeDog);

            this.context.SaveChanges();

            var result = this.usersController.User(fakeUser.Id) as ViewResult;

            Assert.IsAssignableFrom<UserViewModel>(result.Model);

            var resultModel = result.Model as UserViewModel;

            Assert.AreEqual("Manfred Mann", resultModel.Name);

            Assert.AreEqual(2, resultModel.Dogs.Count);

            Assert.AreEqual(1, resultModel.AdoptedDogs.Count);

            Assert.IsTrue(resultModel.Dogs.Any(d => d.Name == "Gligan" && d.Images.Count == 1));

            Assert.IsTrue(resultModel.AdoptedDogs.Single().Name == "NoMoreHomeless");
        }
    }
}
