using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WPR_backend.Data;
using WPR_backend.Models;

namespace WPR_backend.Controllers {
    [Route("api")]
    [ApiController]
    public class AutoController : ControllerBase {
        private readonly ApplicationDbContext _context;

        public AutoController(ApplicationDbContext context) {
            _context = context;
        }

        [HttpGet("autos")]
        public async Task<ActionResult<IEnumerable<Auto>>> GetAllCars()
        {
            var autos = await _context.Autos.ToListAsync();
            return Ok(autos);
        }
    }
}