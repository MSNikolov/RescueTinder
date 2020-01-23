using RescueTinder.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class EditDogViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(3), MaxLength(50)]
        [Display(Name = "Dog name")]
        public string Name { get; set; }

        [Display(Name = "Dog's gender")]
        public Gender Gender { get; set; }

        [Required]
        [Display(Name = "Date of birth")]
        public DateTime BirthDate { get; set; }

        [Required]
        [Display(Name = "Where is the dog currently situated?")]
        public Province Province { get; set; }

        [Display(Name = "Is the dog vaccinated?")]
        public bool IsVaccinated { get; set; }

        [Display(Name = "Is the dog disinfected?")]
        public bool IsDisinfected { get; set; }

        [Display(Name = "Is the dog castrated?")]
        public bool IsCastrated { get; set; }

        [Display(Name = "Additional notes")]
        public string OwnerNotes { get; set; }

        [Required]
        [Display(Name = "Breed")]
        public DogBreed Breed { get; set; }
    }
}
