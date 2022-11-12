using AwesomeCompany.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AwesomeCompany.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public CompanyController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            if (_databaseContext.Companies == null)
            {
                return NotFound();
            }

            return await _databaseContext.Companies.ToListAsync(); 
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            if (_databaseContext.Companies == null)
            {
                return NotFound("The company is yet to be created.");
            }

            var company = await _databaseContext.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound($"The company with Id '{id}' was not found.");
            }

            return company;
        }
    
    }
}
