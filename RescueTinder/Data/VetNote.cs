using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RescueTinder.Data
{
    public class VetNote
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SubjectId { get; set; }

        [Required]
        [ForeignKey(nameof(SubjectId))]
        public Dog Subject { get; set; }

        public string VetId { get; set; }

        [Required]
        [ForeignKey(nameof(VetId))]
        public User Vet { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        [MinLength(50)]
        public string Content { get; set; }
    }
}