using RescueTinder.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class DogViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Gender Gender { get; set; }

        public DateTime BirthDate { get; set; }

        public Province Province { get; set; }

        public bool IsVaccinated { get; set; }

        public bool IsDisinfected { get; set; }

        public bool IsCastrated { get; set; }

        public string Owner { get; set; }

        public string OwnerNotes { get; set; }

        public string Vet { get; set; }

        public string VetId { get; set; }

        public Dictionary<DateTime, string> VetNotes { get; set; } = new Dictionary<DateTime, string>();

        public bool Adopted { get; set; }

        public DogBreed Breed { get; set; }

        public string OwnerId { get; set; }

        public HashSet<LikeViewModel> Likes { get; set; } = new HashSet<LikeViewModel>();

        public Dictionary<Guid, string> Images { get; set; } = new Dictionary<Guid, string>();
    }
}
