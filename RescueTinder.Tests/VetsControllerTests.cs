using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public class VetsControllerTests
    {
        private VetsController vetsController;
        private ApplicationDbContext context;
        private UserManager<User> userManager;
        private RoleManager<IdentityRole> roleManager;

        [SetUp]
        public void CtorShouldWork()
        {

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ResqueDb")
                .Options;


            var db = new ApplicationDbContext(options);

            this.context = db;

            var store = new Mock<IUserStore<User>>();

            var userManager = new Mock<UserManager<User>>(
                    store.Object,
                    new Mock<IOptions<IdentityOptions>>().Object,
                    new Mock<IPasswordHasher<User>>().Object,
                    new IUserValidator<User>[0],
                    new IPasswordValidator<User>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<IServiceProvider>().Object,
                    new Mock<ILogger<UserManager<User>>>().Object);

            this.context.Users.RemoveRange(this.context.Users);

            this.context.SaveChanges();


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

            var roleManager = new Mock<RoleManager<IdentityRole>>(
                    new Mock<IRoleStore<IdentityRole>>().Object,
                    new IRoleValidator<IdentityRole>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

            this.roleManager = roleManager.Object;

            var vetsController = new VetsController(this.userManager, this.context, this.roleManager);

            vetsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            this.vetsController = vetsController;
        }

        [Test]
        public void NewVetShouldAddAVetLicenceNumberButStayUnaproved()
        {
            var number = "0000000000";

            var result = this.vetsController.New(number) as RedirectToActionResult;

            var user = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg");

            Assert.AreEqual(number, user.VetLicence);

            Assert.AreEqual(false, user.VetAprovedByAdmin);
        }

        [Test]
        public void ManageVetsShouldReturnCorrectData()
        {
            var fakeUserOne = new User
            {
                Email = "faker@faker.faker",
                ImageUrl = "imageurl",
                FirstName = "Manol",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.02.2002"),
                VetLicence = "1111111111"
            };

            this.context.Users.Add(fakeUserOne);

            this.context.SaveChanges();

            var fakeUserTwo = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserTwo);

            this.context.SaveChanges();

            var fakeUserThree = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = null
            };

            this.context.Users.Add(fakeUserThree);

            this.context.SaveChanges();

            var result = this.vetsController.Manage() as ViewResult;

            Assert.IsAssignableFrom<List<VetViewModel>>(result.Model);

            var resultModel = result.Model as List<VetViewModel>;

            Assert.AreEqual(2, resultModel.Count);

            Assert.IsTrue(resultModel.Any(r => r.Name == "Manol Denkov" && r.Email == "faker@faker.faker" && r.AprovedByAdmin == false && r.Id == fakeUserOne.Id));
        }

        [Test]
        public void AdminShouldBeAbleToGivePermissionToAVetToAddData()
        {
            var fakeUserOne = new User
            {
                Email = "faker@faker.faker",
                ImageUrl = "imageurl",
                FirstName = "Manol",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.02.2002"),
                VetLicence = "1111111111"
            };

            this.context.Users.Add(fakeUserOne);

            this.context.SaveChanges();

            var fakeUserTwo = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserTwo);

            this.context.SaveChanges();

            var result = this.vetsController.Aprove(fakeUserOne.Id).Result as ViewResult;

            Assert.AreEqual(true, fakeUserOne.VetAprovedByAdmin);

            Assert.AreEqual(false, fakeUserTwo.VetAprovedByAdmin);
        }

        [Test]
        public void AdminShouldBeAbleToStopAVetFromPracticing()
        {
            var fakeUserOne = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserOne);

            this.context.SaveChanges();

            this.vetsController.Aprove(fakeUserOne.Id);

            Assert.AreEqual(true, fakeUserOne.VetAprovedByAdmin);

            var stop = this.vetsController.Stop(fakeUserOne.Id).Result as ViewResult;

            Assert.AreEqual(false, fakeUserOne.VetAprovedByAdmin);
        }

        [Test]
        public void DogOwnerShouldBeAbleToSeeAListOfAprovedVets()
        {
            var fakeDog = new Dog
            {
                Name = "Grojan",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(fakeDog);

            this.context.SaveChanges();

            var fakeUserOne = new User
            {
                Email = "faker@faker.faker",
                ImageUrl = "imageurl",
                FirstName = "Manol",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.02.2002"),
                VetLicence = "1111111111"
            };

            this.context.Users.Add(fakeUserOne);

            this.context.SaveChanges();

            var fakeUserTwo = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserTwo);

            this.context.SaveChanges();

            var fakeUserThree = new User
            {
                Email = "faker3@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolov",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserThree);

            this.context.SaveChanges();

            var result = this.vetsController.Aprove(fakeUserOne.Id).Result as ViewResult;

            var resultTwo = this.vetsController.Aprove(fakeUserTwo.Id).Result as ViewResult;

            var listToChooseFrom = this.vetsController.Choose(fakeDog.Id) as ViewResult;

            Assert.IsAssignableFrom<List<DogChooseVetViewModel>>(listToChooseFrom.Model);

            var resultModel = listToChooseFrom.Model as List<DogChooseVetViewModel>;

            Assert.AreEqual(2, resultModel.Count);

            Assert.IsTrue(resultModel.Any(v => v.DogVet == fakeDog.Id + "tire" + fakeUserOne.Id));
        }

        [Test]
        public void OwnerShouldBeAbleToSelectAVetForHisDog()
        {
            var fakeDog = new Dog
            {
                Name = "Grojan",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(fakeDog);

            this.context.SaveChanges();

            var fakeUserOne = new User
            {
                Email = "faker@faker.faker",
                ImageUrl = "imageurl",
                FirstName = "Manol",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.02.2002"),
                VetLicence = "1111111111"
            };

            this.context.Users.Add(fakeUserOne);

            this.context.SaveChanges();

            var fakeUserTwo = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserTwo);

            this.context.SaveChanges();

            var fakeUserThree = new User
            {
                Email = "faker3@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolov",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserThree);

            this.context.SaveChanges();

            var result = this.vetsController.Aprove(fakeUserOne.Id).Result as ViewResult;

            var resultTwo = this.vetsController.Aprove(fakeUserTwo.Id).Result as ViewResult;

            var listToChooseFrom = this.vetsController.Choose(fakeDog.Id) as ViewResult;

            var resultModel = listToChooseFrom.Model as List<DogChooseVetViewModel>;

            var chosenOne = resultModel.Single(v => v.VetId == fakeUserOne.Id);

            var choose = this.vetsController.Select(chosenOne.DogVet) as ViewResult;

            Assert.AreEqual(fakeUserOne.Id, fakeDog.VetId);
        }

        [Test]
        public void ShouldNotBeAbleToSelectVetIfNotOwnerOfTheDog()
        {
            var fakeOwner = new User
            {
                Email = "fake@owner.bg"
            };

            this.context.Users.Add(fakeOwner);

            this.context.SaveChanges();

            var fakeDog = new Dog
            {
                Name = "Grojan",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "fake@owner.bg").Id
            };

            this.context.Dogs.Add(fakeDog);

            this.context.SaveChanges();

            var fakeUserOne = new User
            {
                Email = "faker@faker.faker",
                ImageUrl = "imageurl",
                FirstName = "Manol",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.02.2002"),
                VetLicence = "1111111111"
            };

            this.context.Users.Add(fakeUserOne);

            this.context.SaveChanges();

            var fakeUserTwo = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserTwo);

            this.context.SaveChanges();

            var fakeUserThree = new User
            {
                Email = "faker3@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolov",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserThree);

            this.context.SaveChanges();

            var result = this.vetsController.Aprove(fakeUserOne.Id).Result as ViewResult;

            var resultTwo = this.vetsController.Aprove(fakeUserTwo.Id).Result as ViewResult;

            var listToChooseFrom = this.vetsController.Choose(fakeDog.Id) as ViewResult;

            var chosenOne = fakeDog.Id + "tire" + fakeUserOne.Id;

            this.vetsController.Select(chosenOne);

            Assert.AreEqual(null, fakeDog.VetId);
        }

        [Test]
        public void ShouldNotBeAbleToSelectAVetIfDogAlreadyHasOne()
        {
            var fakeDog = new Dog
            {
                Name = "Grojan",
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(fakeDog);

            this.context.SaveChanges();

            var fakeUserOne = new User
            {
                Email = "faker@faker.faker",
                ImageUrl = "imageurl",
                FirstName = "Manol",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.02.2002"),
                VetLicence = "1111111111"
            };

            this.context.Users.Add(fakeUserOne);

            this.context.SaveChanges();

            fakeDog.VetId = fakeUserOne.Id;

            this.context.SaveChanges();

            var fakeUserTwo = new User
            {
                Email = "faker2@faker.faker",
                ImageUrl = "imageurl2",
                FirstName = "Manolo",
                LastName = "Denkov",
                BirthDate = DateTime.Parse("22.03.2003"),
                VetLicence = "2222222222"
            };

            this.context.Users.Add(fakeUserTwo);

            this.context.SaveChanges();

            var chosenOne = fakeDog.Id + "tire" + fakeUserTwo.Id;

            var choose = this.vetsController.Select(chosenOne);

            Assert.AreSame(fakeUserOne, fakeDog.Vet);
        }

        [Test]
        public void PatientsShouldReturnTheCorrectDogs()
        {
            var firstFakeDog = new Dog
            {
                Name = "Pancho",
                VetId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id,
                BirthDate = DateTime.Parse("22.11.2001"),
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            firstFakeDog.Images.Add(new Pic
            {
                ImageUrl = "iamgeurl"
            });

            this.context.Dogs.Add(firstFakeDog);

            this.context.SaveChanges();

            var secondFakeDog = new Dog
            {
                Name = "Ivancho",
                VetId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id,
                BirthDate = DateTime.Parse("22.11.2001"),
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            secondFakeDog.Images.Add(new Pic
            {
                ImageUrl = "url"
            });

            this.context.Dogs.Add(secondFakeDog);

            this.context.SaveChanges();

            var thirdFakeDog = new Dog
            {
                Name = "Anastas"
            };

            this.context.Dogs.Add(thirdFakeDog);

            this.context.SaveChanges();

            var result = this.vetsController.Patients() as ViewResult;

            Assert.IsAssignableFrom<List<DogViewModel>>(result.Model);

            var resultModel = result.Model as List<DogViewModel>;

            Assert.AreEqual(2, resultModel.Count);

            Assert.IsTrue(resultModel.Any(d => d.Name == "Pancho"));

            Assert.IsTrue(resultModel.Any(d => d.Name == "Ivancho"));

            Assert.IsFalse(resultModel.Any(d => d.Name == "Anastas"));
        }

        [Test]
        public void EditShouldShowWindowIfUserIsDogsVet()
        {
            var firstFakeDog = new Dog
            {
                Name = "Pancho",
                VetId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id,
                BirthDate = DateTime.Parse("22.11.2001"),
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id,
                IsVaccinated = false,
                IsDisinfected = true
            };

            firstFakeDog.Images.Add(new Pic
            {
                ImageUrl = "iamgeurl"
            });

            this.context.Dogs.Add(firstFakeDog);

            this.context.SaveChanges();

            var result = this.vetsController.Edit(firstFakeDog.Id) as ViewResult;

            Assert.IsAssignableFrom<VetNoteViewModel>(result.Model);

            var resultModel = result.Model as VetNoteViewModel;

            Assert.AreEqual(firstFakeDog.IsDisinfected, resultModel.IsDisinfected);

            Assert.AreEqual(firstFakeDog.IsVaccinated, resultModel.IsVaccinated);
        }

        [Test]
        public void ShouldNotBeAbleToSeeVetNoteScreenIfNotDogsVet()
        {
            var fakeVet = new User
            {
                Email = "fakevet@fakeclinic.fakecountry"
            };

            this.context.Users.Add(fakeVet);

            this.context.SaveChanges();

            var firstFakeDog = new Dog
            {
                Name = "Pancho",
                VetId = this.context.Users.FirstOrDefault(u => u.Email == "fakevet@fakeclinic.fakecountry").Id,
                BirthDate = DateTime.Parse("22.11.2001"),
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            firstFakeDog.Images.Add(new Pic
            {
                ImageUrl = "iamgeurl"
            });

            this.context.Dogs.Add(firstFakeDog);

            this.context.SaveChanges();

            var result = this.vetsController.Edit(firstFakeDog.Id) as RedirectToActionResult;

            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void ShouldNotBeAbleToUpdateHealthStatusIfNotDogsVet()
        {
            var fakeVet = new User
            {
                Email = "fakevet@fakeclinic.fakecountry"
            };

            this.context.Users.Add(fakeVet);

            this.context.SaveChanges();

            var firstFakeDog = new Dog
            {
                Name = "Pancho",
                VetId = this.context.Users.FirstOrDefault(u => u.Email == "fakevet@fakeclinic.fakecountry").Id,
                BirthDate = DateTime.Parse("22.11.2001"),
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            firstFakeDog.Images.Add(new Pic
            {
                ImageUrl = "iamgeurl"
            });

            this.context.Dogs.Add(firstFakeDog);

            this.context.SaveChanges();

            var model = new VetNoteViewModel
            {
                Id = firstFakeDog.Id,
                IsDisinfected = true,
                IsVaccinated = true,
                VetNote = null
            };

            var result = this.vetsController.Edit(model);

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(false, firstFakeDog.IsVaccinated);

            Assert.AreEqual(false, firstFakeDog.IsDisinfected);
        }

        [Test]
        public void DogsVetShouldBeAbleToChangeHealthStatus()
        {
            var fakeVet = new User
            {
                Email = "fakevet@fakeclinic.fakecountry"
            };

            this.context.Users.Add(fakeVet);

            this.context.SaveChanges();

            var firstFakeDog = new Dog
            {
                Name = "Pancho",
                VetId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id,
                BirthDate = DateTime.Parse("22.11.2001"),
                OwnerId = this.context.Users.FirstOrDefault(u => u.Email == "abv@abv.bg").Id
            };

            firstFakeDog.Images.Add(new Pic
            {
                ImageUrl = "iamgeurl"
            });

            this.context.Dogs.Add(firstFakeDog);

            this.context.SaveChanges();

            Assert.AreEqual(0, firstFakeDog.VetNotes.Count);

            var model = new VetNoteViewModel
            {
                Id = firstFakeDog.Id,
                IsDisinfected = true,
                VetNote = "Dog's fine yo"
            };

            var res = this.vetsController.Edit(model);

            var result = res as RedirectToActionResult;

            Assert.AreEqual("Dog", result.ActionName);

            Assert.AreEqual(true, firstFakeDog.IsDisinfected);

            Assert.AreEqual(1, firstFakeDog.VetNotes.Count);

            Assert.IsTrue(firstFakeDog.VetNotes.Any(v => v.Content == "Dog's fine yo"));
        }
    }
}
