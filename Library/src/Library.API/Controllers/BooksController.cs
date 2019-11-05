using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/Books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo =
                _libraryRepository.GetBooksForAuthor(authorId);

            var booksForAuthor =
                Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);
            return Ok(booksForAuthor);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBooksForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo =
                _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor =
                Mapper.Map<BookDto>(bookForAuthorFromRepo);
            return Ok(bookForAuthor);
        }

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
                return BadRequest();
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            var bookEntity = Mapper.Map<Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if(!_libraryRepository.Save()) 
                throw new Exception($"Creating a book for {authorId} failed on save");
            var bookDto = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new { authorId, id = bookDto.Id }, bookDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo =
                _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
                return NotFound();

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Error deleting book with id: {id}");
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book) 
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo =
                _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Updating book {id} for author {authorId} failed on save");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor"
                    , new {authorId, id = bookToReturn.Id}, bookToReturn);
            }

            Mapper.Map(book, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed on save");
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id
            , [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto);

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"An error occured with patch for id: {id}");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor"
                    , new { authorId, id = bookToReturn.Id }, bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch);

            Mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"An error occured with patch for id: {id}");
            }

            return NoContent();
        }
    }
}