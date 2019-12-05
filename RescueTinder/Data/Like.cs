using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Data
{
    public class Like
    {
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public Guid DogId { get; set; }

        [ForeignKey(nameof(DogId))]
        public Dog Dog { get; set; }
    }
}
