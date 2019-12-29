using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RescueTinder.Data;
using RescueTinder.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace RescueTinder.Controllers
{
    public class MessagesController : Controller
    {
        private ApplicationDbContext context;
        private UserManager<User> userManager;

        public MessagesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Authorize]
        [HttpGet("/Messages/About/{id?}")]
        public IActionResult About(string id)
        {
            var ids = id.Split("tire");

            var userId = ids[0];

            var dogId = Guid.Parse(ids[1]);

            var messages = new List<Message>();

            var dog = new Dog();

            using (this.context)
            {
                messages = this.context.Messages.Where(m =>
                m.SubjectId == dogId && m.SenderId == userId && m.ReceiverId == userManager.GetUserId(User) ||
                m.SubjectId == dogId && m.ReceiverId == userId && m.SenderId == userManager.GetUserId(User))
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Include(m => m.Subject)
                    .ToList();

                dog = this.context.Dogs.Include(d => d.Images).Single(d => d.Id == dogId);
            }

            var oldMessages = new List<MessageViewModel>();

            foreach (var mess in messages)
            {
                oldMessages.Add(new MessageViewModel
                {
                    Id = mess.Id,
                    SenderId = mess.SenderId,
                    SenderImageUrl = mess.Sender.ImageUrl,
                    ReceiverId = mess.ReceiverId,
                    ReceiverImageUrl = mess.Receiver.ImageUrl,
                    CreatedOn = mess.CreatedOn,
                    SubjectId = mess.SubjectId,
                    Content = mess.Content
                });
            }

            var result = new MessageListViewModel
            {
                OldMessages = oldMessages,
                DogImageUrl = dog.Images.First().ImageUrl,
                DogName = dog.Name,
                DogOwnerId = dog.OwnerId,
                Id = id,
                Adopted = dog.Adopted
            };

            return View(result);
        }

        [Authorize]
        [HttpPost("/Messages/About/{id?}")]
        public async Task<IActionResult> About(MessageListViewModel model, string id)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            else
            {
                var ids = id.Split("tire");

                var userId = ids[0];

                var dogId = Guid.Parse(ids[1]);

                using (context)
                {

                    var message = new Message
                    {
                        SubjectId = dogId,
                        ReceiverId = userId,
                        Sender = context.Users.Single(u => u.Id == userManager.GetUserId(User)),
                        CreatedOn = DateTime.UtcNow,
                        Content = model.Content
                    };


                    context.Messages.Add(message);

                    await context.SaveChangesAsync();
                }

                return RedirectToAction("About", "Messages", userId + "tire" + dogId);
            }

        }

        [Authorize]
        public IActionResult All()
        {
            var messages = new List<Message>();

            using (context)
            {
                messages = context.Messages
                    .Where(m => m.SenderId == userManager.GetUserId(User) ||
                                m.ReceiverId == userManager.GetUserId(User))
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Include(m => m.Subject).ThenInclude(s => s.Images)
                    .ToList();
            }

            var result = new List<MailViewModel>();

            foreach (var message in messages)
            {
                var user = new User();

                if (message.SenderId == userManager.GetUserId(User))
                {
                    user = message.Receiver;
                }

                else
                {
                    user = message.Sender;
                }


                if (!result.Any(r => r.DogId == message.SubjectId && r.UserId == user.Id))
                {
                    result.Add(new MailViewModel
                    {
                        Id = user.Id + "tire" + message.SubjectId,
                        UserName = user.FirstName + " " + user.LastName,
                        UserId = user.Id,
                        UserImageUrl = user.ImageUrl,
                        DogName = message.Subject.Name,
                        DogId = message.Subject.Id,
                        DogImageUrl = message.Subject.Images.First().ImageUrl
                    });
                }

            }

            return View(result);

        }
    }
}