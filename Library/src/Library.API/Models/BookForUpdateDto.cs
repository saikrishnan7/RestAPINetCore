using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
