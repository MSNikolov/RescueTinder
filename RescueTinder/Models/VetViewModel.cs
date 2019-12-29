using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class VetViewModel
    {
        public string Id { get; set; }

        public string ImageUrl { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public DateTime BirthDate { get; set; }

        public string VetLicence { get; set; }

        public bool AprovedByAdmin { get; set; }
    }
}
