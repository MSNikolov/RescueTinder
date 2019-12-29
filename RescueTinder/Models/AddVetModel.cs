using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class AddVetModel
    {
        [Required]
        [RegularExpression(@"^[0-9]{10}$")]
        [Display(Name = "Please enter the 10-digit number of your vet licence here")]
        public string VetLicence { get; set; }

    }
}
