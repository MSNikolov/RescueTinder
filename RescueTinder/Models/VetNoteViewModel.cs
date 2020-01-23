using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class VetNoteViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Is the dog vaccinated?")]
        public bool IsVaccinated { get; set; }

        [Display(Name = "Is the dog disinfected?")]
        public bool IsDisinfected { get; set; }

        [Display(Name = "Is the dog castrated?")]
        public bool IsCastrated { get; set; }

        [Display(Name = "What is your current observation about the dog?")]
        public string VetNote { get; set; }
    }
}
