using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocShareAPI.Data;
using DocShareAPI.Models;

namespace DocShareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly DataContext _context;

        public DocumentController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Document
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            return await _context.Documents.ToListAsync();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments(int userId)
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            var documents = await _context.Documents.Where(d => d.UserId == userId).ToListAsync();
            return documents;
        }

        // GET: api/Document/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            return document;
        }

        // PUT: api/Document/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, Document document)
        {
            if (id != document.Id)
            {
                return BadRequest();
            }

            var UpdatedAt = DateTimeOffset.UtcNow;
            document.UpdatedAt = UpdatedAt;
            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Document
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Document>> PostDocument(Document document)
        {
            if (_context.Documents == null)
            {
                return Problem("Entity set 'DataContext.Documents'  is null.");
            }

            var user = await _context.Users.FindAsync(document.UserId);
            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDocument", new { id = document.Id }, document);
        }

        // DELETE: api/Document/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            if (_context.Documents == null)
            {
                // return NotFound();
                // return document not found
                return NotFound(new { message = "Document not found" });
            }
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            // return sucess message
            return Ok(new { message = "Document deleted successfully" });
        }

        private bool DocumentExists(int id)
        {
            return (_context.Documents?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
