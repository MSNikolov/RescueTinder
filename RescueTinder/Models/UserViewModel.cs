using RescueTinder.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class UserViewModel
    {
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public Province Province { get; set; }

        public DateTime BirthDate { get; set; }

        public string PersonalNotes { get; set; }

        public List<DogViewModel> Dogs { get; set; } = new List<DogViewModel>();

        public List<DogViewModel> AdoptedDogs { get; set; } = new List<DogViewModel>();
    }
}
