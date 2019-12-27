using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Data
{
    public class Dog 
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MinLength(3), MaxLength(50)]
        public string Name { get; set; }

        public DateTime BirthDate { get; set; }                

        [Required]
        public Province Province { get; set; }
        
        public bool IsVaccinated { get; set; }

        public bool IsDisinfected{ get; set; }

        public string OwnerId { get; set; }

        [Required]
        public User Owner { get; set; }

        public string OwnerNotes { get; set; }

        public string VetId { get; set; }

        [ForeignKey(nameof(VetId))]
        public User Vet { get; set; }

        public HashSet<VetNote> VetNotes { get; set; } = new HashSet<VetNote>();

        public bool Adopted { get; set; }

        [Required]
        public DogBreed Breed { get; set; }

        public HashSet<Like> LikedBy { get; set; } = new HashSet<Like>();

        public HashSet<Message> Messages { get; set; } = new HashSet<Message>();

        public HashSet<Pic> Images { get; set; } = new HashSet<Pic>();
    }
}
