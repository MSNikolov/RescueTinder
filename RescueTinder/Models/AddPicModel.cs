using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class AddPicModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Add a picture of the dog in jpg format")]
        public IFormFile Pic { get; set; }
    }
}
