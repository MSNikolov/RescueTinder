using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RescueTinder.Data
{
    public class User : IdentityUser
    {

        [Required]
        [MinLength(3), MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(3), MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        public string PersonalNotes { get; set; }

        public string VetLicence { get; set; }

        [Required]
        public Province Province { get; set; }

        public HashSet<Dog> Dogs { get; set; } = new HashSet<Dog>();

        public HashSet<Dog> DogPatients { get; set; } = new HashSet<Dog>();

        public HashSet<Like> Liked { get; set; } = new HashSet<Like>();

        public HashSet<Message> SentMessages { get; set; } = new HashSet<Message>();

        public HashSet<Message> ReceivedMessages { get; set; } = new HashSet<Message>();

        public HashSet<VetNote> VetNotes { get; set; } = new HashSet<VetNote>();
    }
}