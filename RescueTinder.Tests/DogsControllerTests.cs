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

    public class DogsControllerTests
    {
        private DogsController dogsController;
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

            var dogsController = new DogsController(this.context, this.userManager);

            dogsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            this.dogsController = dogsController;
        }

        [Test]
        public async Task NewShouldWorkProperly()
        {
            this.createDog.Image = null;     

            await this.dogsController.New(this.createDog);

            Assert.That(this.context.Dogs.Any(d => d.Name == "Pesho" 
            && d.Province == Province.Burgas 
            && d.IsDisinfected == true 
            && d.IsVaccinated == false));
        }

        [Test]
        public void NotJpgImagesShouldNotWork()
        {
            var img = new Mock<IFormFile>();

            var name = "sdfer.abv";

            img.Setup(i => i.FileName).Returns(name);

            this.createDog.Image = img.Object;

            this.createDog.Name = "Kanio";

            this.dogsController.New(this.createDog);

            Assert.That(!this.context.Dogs.Any(d => d.Name == "Kanio"));
        }

        [Test]
        public void NewShouldNotWorkWithInvalidBirthdate()
        {
            this.createDog.OwnerNotes = "Nema";           

            this.createDog.Image = null;

            this.createDog.Name = "Nema";

            this.createDog.BirthDate = DateTime.Parse("22.11.2020");

            this.dogsController.New(this.createDog);

            Assert.That(!this.context.Dogs.Any(d => d.Name == "Nema"));
        }

        [Test]
        public void SearchShouldWorkProperly()
        {
            var model = new SearchDogViewModel
            {
                Province = Province.SofiaCityProvince,
                Breed = DogBreed.Affenpinscher,
                IsDisinfected = false,
                IsVaccinated = false,
                HasVetNotes = false,
                MaxAge = 20,
                MinAge = 0
            };

            var result = dogsController.Search(model) as ViewResult;

            var res = result.Model as List<DogViewModel>;

            Assert.IsInstanceOf < List<DogViewModel>>(res);

            Assert.AreEqual(0, res.Count);

            this.createDog.Name = "Pavel";
            this.createDog.Breed = DogBreed.Affenpinscher;
            this.createDog.BirthDate = DateTime.Parse("20.01.2019");
            this.createDog.Province = Province.SofiaCityProvince;
            this.createDog.IsDisinfected = false;
            this.createDog.IsVaccinated = true;

            this.dogsController.New(this.createDog);

            var resultat = dogsController.Search(model) as ViewResult;

            var final = resultat.Model as List<DogViewModel>;

            Assert.AreEqual(1, final.Count);

            Assert.AreEqual("Pavel", final.First().Name);

            Assert.AreEqual("SearchResults", resultat.ViewName);

            model.IsDisinfected = true;

            var newResult = dogsController.Search(model) as ViewResult;

            var newRes = newResult.Model as List<DogViewModel>;

            Assert.AreEqual(0, newRes.Count);

        }

        [Test]
        public void DogShouldReturnADogViewModel()
        {
            this.createDog.Name = "Galfon";

            this.dogsController.New(this.createDog);

            var dog = this.context.Dogs.Single(d => d.Name == "Galfon");

            var id = dog.Id;

            var result = this.dogsController.Dog(id) as ViewResult;

            Assert.IsAssignableFrom<DogViewModel>(result.Model);

            var dogResult = result.Model as DogViewModel;

            Assert.AreEqual("Galfon", dogResult.Name);
        }

        [Test]
        public void DogShouldNotWorkWithInvalidId()
        {
            this.dogsController.New(this.createDog);

            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var res = this.dogsController.Dog(id) as RedirectToActionResult;

            Assert.AreEqual("Home", res.ControllerName);

            Assert.AreEqual("Index", res.ActionName);
        }

        [Test]
        public void LikeShouldWork()
        {
            var user = new User
            {
                Email = "abvg@abv.bg",
                FirstName = "Pesho",
                LastName = "Peshev",
                BirthDate = DateTime.UtcNow,
                ImageUrl = "ablabl"
            };

            this.context.Users.Add(user);

            this.context.SaveChanges();

            var dog = new Dog
            {
                Name = "Rexi",
                OwnerId = user.Id
            };

            this.context.Dogs.Add(dog);

            this.context.SaveChanges();

            var id = dog.Id;

            Assert.AreEqual(0, dog.LikedBy.Count);

            this.dogsController.Like(id);

            Assert.AreEqual(1, dog.LikedBy.Count);

            Assert.AreEqual("Pesho", dog.LikedBy.First().User.FirstName);
        }

        [Test]
        public void OwnerShouldNotBeAbleToLikeOwnDog()
        {
            this.createDog.Name = "Tanya";

            this.dogsController.New(this.createDog);

            var id = this.context.Dogs.FirstOrDefault(d => d.Name == "Tanya").Id;

            var result = this.dogsController.Like(id).Result as RedirectToActionResult;

            Assert.AreEqual("Dog", result.ActionName);

            Assert.AreEqual(0, this.context.Dogs.Single(d => d.Id == id).LikedBy.Count);
        }

        [Test]
        public void MyShouldWorkCorrectly()
        {
            this.context.Dogs.RemoveRange(context.Dogs);

            this.createDog.Name = "Andon";

            this.dogsController.New(this.createDog);

            this.createDog.Name = "Bratan";

            this.dogsController.New(this.createDog);

            var result = this.dogsController.My() as ViewResult;

            Assert.IsAssignableFrom<List<DogViewModel>>(result.Model);

            var resultList = result.Model as List<DogViewModel>;

            Assert.AreEqual(2, resultList.Count);

            Assert.IsTrue(resultList.Any(d => d.Name == "Bratan"));
        }

        [Test]
        public void EditGetterShouldReturnEditDogViewModel()
        {
            this.createDog.Name = "Petya";

            this.dogsController.New(this.createDog);

            var editRes = this.dogsController.Edit(this.context.Dogs.Single(d => d.Name == "Petya").Id);

            var res = editRes as ViewResult;

            Assert.IsAssignableFrom<EditDogViewModel>(res.Model);

            var result = res.Model as EditDogViewModel;

            var viewResult = editRes as ActionResult;

            Assert.AreEqual("Petya", result.Name);
        }

        [Test]
        public void ShouldNotBeAbleToSeeEditPageIfNotOwner()
        {
            this.createDog.Name = "Mustafa";

            this.dogsController.New(this.createDog);

            var userFaker = new User
            {
                FirstName = "FakerFaker"
            };

            this.context.Users.Add(userFaker);

            this.context.SaveChanges();

            var id = this.context.Dogs.Single(d => d.Name == "Mustafa").Id;

            this.context.Dogs.Single(d => d.Id == id).Owner = userFaker;

            this.context.SaveChanges();

            var result = this.dogsController.Edit(id) as RedirectToActionResult;

            Assert.AreEqual("Home", result.ControllerName);

            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void EditShouldWorkCorrectly()
        {
            this.createDog.Name = "Anastasya";
            this.createDog.Province = Province.Montana;

            this.dogsController.New(this.createDog);

            var dog = this.context.Dogs.Single(d => d.Name == "Anastasya");

            var model = new EditDogViewModel
            {
                Id = dog.Id,
                Name = "KOlyo",
                Province = dog.Province
            };

            this.dogsController.Edit(model);

            Assert.AreEqual("KOlyo", dog.Name);

            Assert.AreEqual(Province.Montana, dog.Province);
        }

        [Test]
        public void ShouldNotBeAbleToEditIfNotOwner()
        {
            this.createDog.Name = "Balkan";

            this.dogsController.New(createDog);

            var fakeFaker = new User
            {
                FirstName = "Faker"
            };

            this.context.Users.Add(fakeFaker);

            this.context.SaveChanges();

            var dogId = this.context.Dogs.Single(d => d.Name == "Balkan").Id;

            this.context.Dogs.Single(d => d.Id == dogId).Owner = fakeFaker;

            this.context.SaveChanges();

            var model = new EditDogViewModel
            {
                Id = dogId,
                Name = "Zofran"
            };

            var result = this.dogsController.Edit(model) as RedirectToActionResult;

            Assert.AreEqual("Index", result.ActionName);

            Assert.AreEqual("Balkan", this.context.Dogs.Single(d => d.Id == dogId).Name);
        }

        [Test]
        public void ShouldNotBeAbleToGiveForAdoptionIfNotOwner()
        {
            this.createDog.Name = "Dragan";

            this.dogsController.New(this.createDog);

            var faker = new User
            {
                FirstName = "Mitko",
                LastName = "Petrov",
                Email = "mitkopetrov@alavala.bg",
                Province = Province.Blagoevgrad,
                ImageUrl = "sdofi"
            };

            this.context.Users.Add(faker);

            this.context.SaveChanges();

            var userId = faker.Id;

            var dogId = this.context.Dogs.Single(d => d.Name == "Dragan").Id;

            this.context.Dogs.Single(d => d.Id == dogId).Owner = faker;

            this.context.SaveChanges();

            var id = userId + "tire" + dogId;

            var result = this.dogsController.Adopt(id) as RedirectToActionResult;

            Assert.AreEqual("Index", result.ActionName);

            Assert.IsFalse(this.context.Dogs.Single(d => d.Id == dogId).Adopted);
        }

        [Test]
        public void OwnerShouldBeAbleToAllowAdoption()
        {
            var faker = new User
            {
                FirstName = "Faker"
            };

            this.context.Users.Add(faker);

            this.context.SaveChanges();

            this.createDog.Name = "Gligan";

            this.dogsController.New(this.createDog);

            var userId = faker.Id;

            var dogId = this.context.Dogs.Single(d => d.Name == "Gligan").Id;

            var id = userId + "tire" + dogId;

            this.dogsController.Adopt(id);

            Assert.AreEqual(faker, this.context.Dogs.Single(d => d.Id == dogId).Owner);

            Assert.IsTrue(this.context.Dogs.Single(d => d.Id == dogId).Adopted);
        }
    }
}
