using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NWUTechTrends.Models;

namespace NWUTechTrends.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class JobTelemetriesController : ControllerBase
    {
        private readonly TechtrendsContext _context;

        public JobTelemetriesController(TechtrendsContext context)
        {
            _context = context;
        }

        // GET: api/JobTelemetries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobTelemetry>>> GetJobTelemetries()
        {
            return await _context.JobTelemetries.ToListAsync();
        }

        // GET: api/JobTelemetries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobTelemetry>> GetJobTelemetry(int id)
        {
            var jobTelemetry = await _context.JobTelemetries.FindAsync(id);

            if (jobTelemetry == null)
            {
                return NotFound();
            }

            return jobTelemetry;
        }

        // PUT: api/JobTelemetries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJobTelemetry(int id, JobTelemetry jobTelemetry)
        {
            if (id != jobTelemetry.Id)
            {
                return BadRequest();
            }

            if (!JobTelemetryExists(id))
            {
                return NotFound("Telemetry not found.");
            }

            _context.Entry(jobTelemetry).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobTelemetryExists(id))
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

        // POST: api/JobTelemetries
        [HttpPost]
        public async Task<ActionResult<JobTelemetry>> PostJobTelemetry(JobTelemetry jobTelemetry)
        {
            _context.JobTelemetries.Add(jobTelemetry);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobTelemetry", new { id = jobTelemetry.Id }, jobTelemetry);
        }

        // DELETE: api/JobTelemetries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobTelemetry(int id)
        {
            if (!JobTelemetryExists(id))
            {
                return NotFound("Telemetry not found.");
            }

            var jobTelemetry = await _context.JobTelemetries.FindAsync(id);
            if (jobTelemetry == null)
            {
                return NotFound();
            }

            _context.JobTelemetries.Remove(jobTelemetry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Private method to check if a JobTelemetry exists by ID
        private bool JobTelemetryExists(int id)
        {
            return _context.JobTelemetries.Any(e => e.Id == id);
        }

        // GET: api/JobTelemetries/savings
        [HttpGet("savingsByProject")]
        public async Task<ActionResult<SavingsResultProject>> GetSavingsByProject(
            [FromQuery] Guid projectId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date cannot be after end date.");
                }

                var savings = await _context.JobTelemetries
                    .Join(_context.Processes,
                          jt => jt.ProccesId,
                          p => p.ProcessId.ToString(),
                          (jt, p) => new { jt, p })
                    .Where(joined => joined.p.ProjectId == projectId &&
                                     joined.jt.EntryDate >= startDate &&
                                     joined.jt.EntryDate <= endDate)
                    .GroupBy(joined => joined.p.ProjectId)
                    .Select(g => new
                    {
                        ProjectId = g.Key,
                        TotalTimeSaved = g.Sum(j => j.jt.HumanTime ?? 0),
                        TotalCostSaved = g.Sum(j => 0m) // Placeholder for cost saved, as there's no cost field in the schema
                    })
                    .FirstOrDefaultAsync();

                if (savings == null)
                {
                    return NotFound("No data found for the given parameters.");
                }

                return Ok(new SavingsResultProject
                {
                    ProjectId = (Guid)savings.ProjectId,
                    TotalTimeSaved = savings.TotalTimeSaved,
                    TotalCostSaved = savings.TotalCostSaved // Placeholder, adjust as needed
                });
            }
            catch (Exception ex)
            {
             
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving savings data from the database");
            }
        }
        [HttpGet("savingsByClient")]
        public async Task<ActionResult<SavingsResultClient>> GetSavingsByClient(
            [FromQuery] Guid clientId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date cannot be after end date.");
                }

                // Query the database to calculate the total time and cost saved for the specified client
                var savings = await _context.JobTelemetries
                    .Join(_context.Processes,
                          jt => jt.ProccesId,
                          p => p.ProcessId.ToString(),
                          (jt, p) => new { jt, p })
                    .Join(_context.Projects,
                          jp => jp.p.ProjectId,
                          pr => pr.ProjectId,
                          (jp, pr) => new { jp.jt, jp.p, pr.ClientId })
                    .Where(joined => joined.ClientId == clientId &&
                                     joined.jt.EntryDate >= startDate &&
                                     joined.jt.EntryDate <= endDate)
                    .GroupBy(joined => joined.ClientId)
                    .Select(g => new
                    {
                        ClientId = g.Key,
                        TotalTimeSaved = g.Sum(j => j.jt.HumanTime ?? 0),
                        TotalCostSaved = g.Sum(j => 0m) // Placeholder for cost saved, as there's no cost field in the schema
                    })
                    .FirstOrDefaultAsync();

                if (savings == null)
                {
                    return NotFound("No data found for the given parameters.");
                }

                return Ok(new SavingsResultClient
                {
                    ClientId = (Guid)savings.ClientId,
                    TotalTimeSaved = savings.TotalTimeSaved,
                    TotalCostSaved = savings.TotalCostSaved // Placeholder, adjust as needed
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving savings data from the database");
            }
        }



        public class SavingsResultProject
        {
            public Guid ProjectId { get; set; }
            public int TotalTimeSaved { get; set; }
            public decimal TotalCostSaved { get; set; } // Placeholder, adjust as needed
        }
        public class SavingsResultClient
        {
            public Guid ClientId { get; set; }
            public int TotalTimeSaved { get; set; }
            public decimal TotalCostSaved { get; set; } // Placeholder, adjust as needed
        }
    }
}
