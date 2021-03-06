﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RescueTinder.Models
{
    public class MailViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string UserId { get; set; }

        public string UserImageUrl { get; set; }

        public string DogName { get; set; }

        public Guid DogId { get; set; }

        public string DogImageUrl { get; set; }
    }
}
