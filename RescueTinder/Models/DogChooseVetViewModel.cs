using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class DogChooseVetViewModel
    {
        public Guid DogId { get; set; }

        public string VetId { get; set; }

        public string VetName { get; set; }

        public string VetImageUrl { get; set; }

        public string DogVet => DogId + "tire" + VetId;
    }
}
