using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnetapp.Data;
using dotnetapp.Models;

namespace dotnetapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Doctor
        [HttpPost]
        public async Task<ActionResult<Doctor>> CreateDoctor([FromBody] Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, doctor);
        }

        // GET: api/Doctor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
        {
            return await _context.Doctors.Include(d => d.Appointments).ToListAsync();
        }

        // GET: api/Doctor/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return doctor;
        }

        // GET: api/Doctor/sortedbyfee
        [HttpGet("sortedbyfee")]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctorsSortedByFeeDescending()
        {
            var doctors = await _context.Doctors
                .OrderByDescending(d => ConvertToDecimal(d.DoctorFee)) // Assumes DoctorFee is a string representing a decimal
                .Include(d => d.Appointments)
                .ToListAsync();

            return Ok(doctors);
        }

        // Utility method to convert DoctorFee to decimal
        private decimal ConvertToDecimal(string fee)
        {
            // Handle conversion; assume fee is in a valid decimal format
            if (decimal.TryParse(fee, out var result))
            {
                return result;
            }

            // Return a default value or handle invalid conversion
            return 0m;
        }
    }
}
