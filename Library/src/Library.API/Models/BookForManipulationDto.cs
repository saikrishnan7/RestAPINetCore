using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [Required(ErrorMessage = "Please fill out a title.")]
        [MaxLength(100, ErrorMessage = "Max length should be less than 100 chars.")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Max length should be less than 500 chars.")]
        public virtual string Description { get; set; }
    }
}
