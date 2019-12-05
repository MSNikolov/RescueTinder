using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RescueTinder.Data
{
    public class Pic
    {
        public Guid Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public Guid DogId { get; set; }

        [ForeignKey(nameof(DogId))]
        public Dog Dog { get; set; }
    }
}