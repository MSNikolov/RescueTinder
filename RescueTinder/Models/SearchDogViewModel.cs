using RescueTinder.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class SearchDogViewModel
    {
        [Range(0, 25)]
        [Display(Name = "Select minimum age")]
        public int MinAge { get; set; }

        [Range(0, 25)]
        [Display(Name = "Select maximum age")]
        public int MaxAge { get; set; }

        [Display(Name = "Select dog's gender")]
        public Gender Gender { get; set; }

        [Display(Name = "Select province")]
        public Province Province { get; set; }

        [Display(Name = "Select breed")]
        public DogBreed Breed { get; set; }

        [Display(Name = "Only vaccinated dogs")]
        public bool IsVaccinated { get; set; }

        [Display(Name = "Only disinfected dogs")]
        public bool IsDisinfected { get; set; }

        [Display(Name = "Only castrated dogs")]
        public bool IsCastrated { get; set; }

        [Display(Name = "Only dogs with notes from the vet")]
        public bool HasVetNotes { get; set; }
    }
}
