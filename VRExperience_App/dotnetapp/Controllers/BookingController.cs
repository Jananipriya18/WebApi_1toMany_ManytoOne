using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnetapp.Exceptions;
using dotnetapp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetapp.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking/ExperienceEnrollmentForm/5
        public IActionResult ExperienceEnrollmentForm(int VRExperienceID)
        {
            var experience = _context.VRExperiences.Find(VRExperienceID);
            if (experience == null)
            {
                return NotFound();
            }

            return View(experience);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExperienceEnrollmentForm(int VRExperienceID, Attendee attendee)
        {
            var experience = await _context.VRExperiences
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.VRExperienceID == VRExperienceID);

            if (experience == null)
            {
                return NotFound();
            }

            if (experience.Attendees.Count >= experience.MaxCapacity)
            {
                throw new VRExperienceBookingException("Maximum Number Reached");
            }

            if (!ModelState.IsValid)
            {
                return View(attendee); // Returns the view with the Attendee model
            }

            attendee.VRExperienceID = VRExperienceID;
            _context.Attendees.Add(attendee);
            experience.MaxCapacity -= 1; // Reduce the capacity
            await _context.SaveChangesAsync();

            return RedirectToAction("EnrollmentConfirmation", new { AttendeeID = attendee.AttendeeID });
        }


        public async Task<IActionResult> EnrollmentConfirmation(int AttendeeID)
        {
            var attendee = await _context.Attendees
                .Include(a => a.VRExperience)
                .SingleOrDefaultAsync(a => a.AttendeeID == AttendeeID);

            if (attendee == null)
            {
                return NotFound();
            }

            return View(attendee); // Ensure this view expects an Attendee model
        }

    }
}
