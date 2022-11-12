using AwesomeCompany.Entities;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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


        [Route("GetCompanies")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            if (_databaseContext.Companies == null)
            {
                return NotFound();
            }

            return await _databaseContext.Companies.ToListAsync(); 
        }

        [Route("getcompany/{id}")]
        [HttpGet]
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

        [Route("IncreaseSalaries/{id}")]
        [HttpPut]
        public async Task<ActionResult<Company>> IncreaseSalaries(int id)
        { 
            var company = await _databaseContext
                                        .Set<Company>()
                                        .Include(c => c.Employees)
                                        .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound($"The company with Id '{id}' was not found.");
            }

            foreach(var employee in company.Employees)
            {
                employee.Salary *= 1.1m;
            }

            company.LastSalaryUpdate= DateTime.UtcNow;

            await _databaseContext.SaveChangesAsync();

            return NoContent();
        }

        [Route("IncreaseSalariesSql/{id}")]
        [HttpPut]
        public async Task<ActionResult<Company>> IncreaseSalariesSql(int id)
        {
            var company = await _databaseContext
                                        .Set<Company>() 
                                        .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound($"The company with Id '{id}' was not found.");
            }

            await _databaseContext.Database.BeginTransactionAsync();

            await _databaseContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Employees SET Salary = Salary * 1.1 where Id = {company.Id}"); 

            company.LastSalaryUpdate = DateTime.UtcNow;

            await _databaseContext.SaveChangesAsync();

            await _databaseContext.Database.CommitTransactionAsync();

            return NoContent();
        }

        [Route("IncreaseSalariesSqlDapper/{id}")]
        [HttpPut]
        public async Task<ActionResult<Company>> IncreaseSalariesSqlDapper(int id)
        {
            var company = await _databaseContext
                                        .Set<Company>()
                                        .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound($"The company with Id '{id}' was not found.");
            }

            var transaction = await _databaseContext.Database.BeginTransactionAsync();

            await _databaseContext.Database.GetDbConnection().ExecuteAsync(
                "UPDATE Employees SET Salary = Salary * 1.1 WHERE Id = @Id",
                new { Id = company.Id },
                transaction.GetDbTransaction()
                );

            company.LastSalaryUpdate = DateTime.UtcNow;

            await _databaseContext.SaveChangesAsync();

            await _databaseContext.Database.CommitTransactionAsync();

            return NoContent();
        }

    }
}
