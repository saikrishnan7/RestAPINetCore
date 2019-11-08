using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class BookForUpdateDto : BookForManipulationDto
    {
        [Required(ErrorMessage = "A description is required.")]
        public override string Description
        {
            get => base.Description;
            set => base.Description = value;
        }
    }
}
