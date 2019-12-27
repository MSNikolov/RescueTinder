using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class MessageViewModel
    {
        public Guid Id { get; set; }

        public string DogName { get; set; }

        public string SenderId { get; set; }

        public string SenderImageUrl { get; set; }

        public string ReceiverId { get; set; }

        public string ReceiverImageUrl { get; set; }

        public Guid SubjectId { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }
    }
}
