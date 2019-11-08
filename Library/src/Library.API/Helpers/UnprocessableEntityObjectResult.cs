using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.API.Helpers
{
    public class UnprocessableEntityObjectResult : ObjectResult
    {
        public UnprocessableEntityObjectResult(ModelStateDictionary modelState) : 
            base(new SerializableError(modelState ?? throw new ArgumentNullException(nameof(modelState))))
        {
            StatusCode = 422;
        }
    }
}
