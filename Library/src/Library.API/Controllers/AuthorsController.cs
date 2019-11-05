using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    [Route("api/Authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private IUrlHelper _urlHelper;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);
            var previousPageLink = authorsFromRepo.HasPrevious
                ? CreateAuthorsResourceUri(authorsResourceParameters
                    , ResourceUriType.PreviousPage)
                : null;
            var nextPageLink = authorsFromRepo.HasPrevious
                ? CreateAuthorsResourceUri(authorsResourceParameters
                    , ResourceUriType.NextPage)
                : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount
                , pageSize = authorsFromRepo.PageSize
                , currentPage = authorsFromRepo.CurrentPage
                , totalPages = authorsFromRepo.TotalPages
                , previousPageLink
                , nextPageLink
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors);
        }

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters
            , ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        genre = authorsResourceParameters.Genre,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize,
                        genre = authorsResourceParameters.Genre,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
                default:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        genre = authorsResourceParameters.Genre,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
            }
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
                return NotFound();
            return Ok(Mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost()]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(authorEntity);
            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            return _libraryRepository.AuthorExists(id) ? 
                new StatusCodeResult(StatusCodes.Status409Conflict) : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
                return NotFound();

            _libraryRepository.DeleteAuthor(authorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Error deleting author with id: {id}");
            }

            return NoContent();
        }
    }
}