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
    public class MessagesControllerTests
    {
        private MessagesController mController;
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

            var mController = new MessagesController(this.context, this.userManager);

            mController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            this.mController = mController;
        }

        [Test]
        public void AboutShouldDisplayCorrectData()
        {
            var faker = new User
            {
                Email = "faker@fakefaker.fake",
                ImageUrl = "fakephoto"
            };

            this.context.Users.Add(faker);

            this.context.SaveChanges();

            var fakeDog = new Dog
            {
                Name = "Petkan",
                OwnerId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id
            };

            fakeDog.Images.Add(new Pic
            {
                Dog = fakeDog,
                ImageUrl = "fakepic"
            });

            this.context.Dogs.Add(fakeDog);

            this.context.SaveChanges();

            for (int i = 0; i < 5; i++)
            {
                var mess = new Message
                {
                    Content = "Hellokitty",
                    SenderId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id,
                    ReceiverId = faker.Id,
                    SubjectId = fakeDog.Id,
                    CreatedOn = DateTime.UtcNow
                };

                this.context.Messages.Add(mess);

                this.context.SaveChanges();                
            }

            var id = faker.Id + "tire" + fakeDog.Id;

            var messages = this.mController.About(id) as ViewResult;

            Assert.IsAssignableFrom<MessageListViewModel>(messages.Model);

            var result = messages.Model as MessageListViewModel;

            Assert.AreEqual(5, result.OldMessages.Count);

            var newMess = new Message
            {
                Content = "Hellodoggy",
                ReceiverId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id,
                SenderId = faker.Id,
                SubjectId = fakeDog.Id,
                CreatedOn = DateTime.UtcNow
            };

            this.context.Messages.Add(newMess);

            this.context.SaveChanges();

            var newFakeDog = new Dog
            {
                Name = "Malin",
                OwnerId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(newFakeDog);

            this.context.SaveChanges();

            var newerMess = new Message
            {
                Content = "Kostenurka",
                SenderId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id,
                ReceiverId = faker.Id,
                SubjectId = newFakeDog.Id,
                CreatedOn = DateTime.UtcNow
            };

            this.context.Messages.Add(newerMess);

            this.context.SaveChanges();

            var resultTwo = this.mController.About(id) as ViewResult;

            var resultTwoModel = resultTwo.Model as MessageListViewModel;

            Assert.AreEqual(6, resultTwoModel.OldMessages.Count);

            Assert.AreEqual("Petkan", resultTwoModel.DogName);
        }

        [Test]
        public void PostingAMessageShouldRunCorrectly()
        {
            var faker = new User
            {
                Email = "fakemail@fake.mail",
                ImageUrl = "fakeimage"
            };

            this.context.Users.Add(faker);

            this.context.SaveChanges();

            var fakeDog = new Dog
            {
                Name = "Kalin",
                OwnerId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id
            };

            this.context.Dogs.Add(fakeDog);

            this.context.SaveChanges();

            var id = faker.Id + "tire" + fakeDog.Id;

            var messageToSend = new MessageListViewModel
            {
                Content = "zdr ko pr",
                Id = id
            };

            var res = this.mController.About(messageToSend, id).Result;

            var result = res as RedirectToActionResult;

            Assert.AreEqual("About", result.ActionName);

            Assert.IsTrue(this.context.Messages.Any(m => m.Content == "zdr ko pr" && m.Sender.Email == "abv@abv.bg"
            && m.Subject == fakeDog
            && m.Receiver == faker));

            Assert.AreEqual(1, this.context.Messages.Where(m => m.Content == "zdr ko pr").Count());
        }

        [Test]
        public void AllMessagesShouldReturnCorrectData()
        {
            this.context.Messages.RemoveRange(this.context.Messages);

            this.context.SaveChanges();

            var faker = new User
            {
                Email = "fakerfaker@faker.faker",
                ImageUrl = "fakersimage",
                FirstName = "Stoyan",
                LastName = "Mushmov"
            };

            this.context.Users.Add(faker);

            this.context.SaveChanges();

            var doggo = new Dog
            {
                Name = "Uzun",
                OwnerId = faker.Id
            };

            doggo.Images.Add(new Pic
            {
                ImageUrl = "doggourl"
            });

            this.context.Dogs.Add(doggo);

            this.context.SaveChanges();

            var firstMess = new Message
            {
                Subject = doggo,
                SenderId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id,
                ReceiverId = faker.Id,
                Content = "blabla",
                CreatedOn = DateTime.UtcNow
            };

            this.context.Messages.Add(firstMess);

            this.context.SaveChanges();

            var secondMess = new Message
            {
                Subject = doggo,
                SenderId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id,
                ReceiverId = faker.Id,
                Content = "blablaba",
                CreatedOn = DateTime.UtcNow
            };

            this.context.Messages.Add(secondMess);

            this.context.SaveChanges();

            var thirdMess = new Message
            {
                Subject = doggo,
                ReceiverId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id,
                SenderId = faker.Id,
                Content = "blablabla",
                CreatedOn = DateTime.UtcNow
            };

            this.context.Messages.Add(thirdMess);

            this.context.SaveChanges();

            var secondFaker = new User
            {
                Email = "secondfaker@faker.faker",
                ImageUrl = "second",
                FirstName = "Stoimen",
                LastName = "Panchev"
            };

            this.context.Users.Add(secondFaker);

            this.context.SaveChanges();

            var secondDoggo = new Dog
            {
                Name = "Nikodim",
                OwnerId = secondFaker.Id
            };

            secondDoggo.Images.Add(new Pic
            {
                Dog = secondDoggo,
                ImageUrl = "seconddd"
            });

            this.context.Dogs.Add(secondDoggo);

            this.context.SaveChanges();

            var fourthMess = new Message
            {
                SenderId = secondFaker.Id,
                ReceiverId = this.context.Users.First(u => u.Email == "abv@abv.bg").Id,
                SubjectId = secondDoggo.Id,
                Content = "blamblam",
                CreatedOn = DateTime.UtcNow
            };

            this.context.Messages.Add(fourthMess);

            this.context.SaveChanges();

            var fifthMess = new Message
            {
                SenderId = secondFaker.Id,
                ReceiverId = faker.Id,
                SubjectId = secondDoggo.Id,
                Content = "blamblam",
                CreatedOn = DateTime.UtcNow
            };

            this.context.Messages.Add(fifthMess);

            this.context.SaveChanges();

            var result = this.mController.All() as ViewResult;

            Assert.IsAssignableFrom<List<MailViewModel>>(result.Model);

            var resultModel = result.Model as List<MailViewModel>;

            Assert.AreEqual(2, resultModel.Count);

            Assert.IsTrue(resultModel.Any(r => r.DogName == doggo.Name && r.UserName == "Stoyan Mushmov"));

            Assert.IsTrue(resultModel.Any(r => r.DogName == secondDoggo.Name && r.UserName == secondFaker.FirstName + " " + secondFaker.LastName));
        }
    }
}
