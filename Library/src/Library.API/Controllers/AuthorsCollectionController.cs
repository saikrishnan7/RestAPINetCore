using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/AuthorCollections")]
    public class AuthorsCollectionController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsCollectionController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet("({ids})", Name = "GetAuthorsCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]
            IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorIds = ids as Guid[] ?? ids.ToArray();
            var authorEntities = _libraryRepository.GetAuthors(authorIds);

            if (authorIds.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn =
                Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }

        [HttpPost()]
        public IActionResult CreateAuthorCollection(
            [FromBody] IEnumerable<AuthorForCreationDto> authorsCollection)
        {
            if (authorsCollection == null)
            {
                return BadRequest();
            }

            var authorEntities =
                Mapper.Map<IEnumerable<Author>>(authorsCollection);
            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Adding a new list of authors failed");
            }

            var authorsCollectionToReturn =
                Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString =
                string.Join(",", authorsCollectionToReturn.Select(a => a.Id));
            
            return CreatedAtRoute("GetAuthorsCollection", new { ids = idsAsString }, 
                authorsCollectionToReturn);
        }
    }
}