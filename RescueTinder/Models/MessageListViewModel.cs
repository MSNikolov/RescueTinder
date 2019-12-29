using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class MessageListViewModel
    {
        public List<MessageViewModel> OldMessages { get; set; } = new List<MessageViewModel>();

        [Required]
        public string Content { get; set; }

        public DateTime CreatedOn { get; set; }

        public string ReceiverId { get; set; }

        public Guid SubjectId { get; set; }

        public string DogName { get; set; }

        public string DogImageUrl { get; set; }

        public string DogOwnerId { get; set; }

        public string Id { get; set; }

        public bool Adopted { get; set; }
    }
}
