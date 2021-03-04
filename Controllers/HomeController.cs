using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExamProject.Models;
using ExamProject.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ExamProject.Controllers
{
    public class HomeController : Controller
    {        
        private static MyContext _context;
        public HomeController(MyContext context)
        {
            _context = context;
        }


//! ------------------------------------------------   Login & Registration   ------------------------------------------------ !//
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return View();
            }
            return RedirectToAction("Dashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult RegisterUser(User user)
        {
            if (ModelState.IsValid)
            {
                List<User> Users = _context.Users
                    .ToList();

                foreach (User PulledUser in Users)
                {
                    if (user.Email == PulledUser.Email)
                    {
                        ModelState.AddModelError("Email", "Email is being used by another account.");
                        return View("Index");
                    }
                }
                // Initializing a PasswordHasher object, provIding our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                //Save your user object to the database
                _context.Users.Add(user);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return RedirectToAction("Success");
            } 
            else 
            {
                return View("Index");
            }
        }


        [HttpPost("auth")]
        public IActionResult Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                User pulledUser = _context.Users.FirstOrDefault(p => p.Email.Contains(user.LoginEmail));
                if (pulledUser == null) 
                {
                    ModelState.AddModelError("LoginEmail", "Email/Password InvalId");
                    return View("Index");
                }
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                // verify provIded password against hash stored in db
                var result = hasher.VerifyHashedPassword(user, pulledUser.Password, user.LoginPassword);
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("LoginPassword", "Email/password InvalId");
                    return View("Index");
                }
                else {
                    HttpContext.Session.SetInt32("UserId", pulledUser.UserId);
                    return RedirectToAction("Success");
                }
            }
            else
            {
                return View("Index");
            }
        }

        //---------------------------------Success / Logout / Errors

        [HttpGet("success")]
        public IActionResult Success()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            ViewBag.UserId = (int)HttpContext.Session.GetInt32("UserId");
            return Redirect("Dashboard");
            // return View();
        }
    

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        //! ------------------------------------------------   Main Website   ------------------------------------------------ !//

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            ViewBag.LoggedUser = _context.Users
                .FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("UserId"));

            List<DojoActivity> DojoActivities = _context.DojoActivities
                .Include(w => w.Participants)
                .Include(w => w.Coordinator)
                .OrderBy(w => w.DateAndTime)
                .ToList();

            foreach (DojoActivity dojoActivity in DojoActivities)
            {
                if (dojoActivity.DateAndTime <= DateTime.Now)
                {
                    dojoActivity.IsArchived = true;
                    _context.SaveChanges();
                }
            }

            ViewBag.Participating = _context.Associations
                .Include(g => g.DojoEvent)
                .Include(g => g.Participant)
                .Where(g => g.Participant.UserId == (int)HttpContext.Session.GetInt32("UserId"))
                .ToList();
            return View(DojoActivities);
        }

        [HttpPost("DojoActivities/AddNewActivity")]
        public IActionResult AddNewActivity()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            return View();
        }

        [HttpPost("Activities/AddNewActivity/Auth")]
        public IActionResult CreateActivity(DojoActivity activity)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            if (ModelState.IsValid)
            {
                User User = _context.Users
                    .FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("UserId"));

                if (activity.DateAndTime <= DateTime.Now)
                {
                    ModelState.AddModelError("DateAndTime", "Event Date must be in the future.");
                    return View("AddNewActivity");
                }

                activity.Coordinator = User;
                _context.Add(activity);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            } 
            else
            {
                return View("AddNewActivity");
            }
        }

        [HttpGet("DojoActivities/{DojoActivityId}")]
        public IActionResult Profile(int DojoActivityId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            ViewBag.LoggedUser = _context.Users
                .FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("UserId"));

            ViewBag.Participants = _context.Associations
                .Include(an => an.Participant)
                .Include(an => an.DojoEvent)
                .Where(an => an.DojoEventId == DojoActivityId)
                .ToList();

            DojoActivity activity = _context.DojoActivities
                .Include(da => da.Coordinator)
                .Include(da => da.Participants)
                .FirstOrDefault(w => w.DojoActivityId == DojoActivityId);

            return View(activity);
        }

        [HttpPost("DojoActivities/{DojoActivityId}/Delete")]
        public IActionResult DeleteWedding(DojoActivity activity)
        {  
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            _context.DojoActivities.Remove(activity);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpPost("DojoActivities/{DojoActivityId}/RSVP")]
        public IActionResult RSVP(int DojoActivityId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            // Get the Current Wedding
            DojoActivity activity = _context.DojoActivities
                .Include(w => w.Participants)
                    .ThenInclude(g => g.Participant)
                .FirstOrDefault(w => w.DojoActivityId == DojoActivityId);
            // Get the current User.
            User CurrUser = _context.Users
                .FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("UserId"));

            // Create a dummy Association object
            Association PulledUser = _context.Associations
                .Include(an => an.DojoEvent)
                .Include(an => an.Participant)
                .Where(an => an.DojoEventId == DojoActivityId)
                .FirstOrDefault(an => an.Participant == CurrUser);
            
            // Add the new Association to the list
            if (PulledUser == null)
            {   
                Association AddUser = new Association() {ParticipantId = CurrUser.UserId, DojoEventId = DojoActivityId, Participant = CurrUser};
                _context.Add(AddUser);
                _context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--------------------Already RSVP'd!--------------------");
            }
            // Redirect from a successful post request
            return RedirectToAction("Dashboard");
        }

        [HttpPost("DojoActivities/{DojoActivityId}/UnRSVP")]
        public IActionResult UNRSVP(int DojoActivityId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            // Get the Current Wedding
            DojoActivity activity = _context.DojoActivities
                .Include(w => w.Participants)
                    .ThenInclude(g => g.Participant)
                .FirstOrDefault(w => w.DojoActivityId == DojoActivityId);
            // Get the current User.
            User CurrUser = _context.Users
                .FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("UserId"));

            // Get the User Association Object
            Association PulledUser = _context.Associations
                .Include(an => an.DojoEvent)
                .Include(an => an.Participant)
                .Where(an => an.DojoEventId == DojoActivityId)
                .FirstOrDefault(an => an.Participant == CurrUser);

            // Add the new Association to the list
            if (PulledUser != null)
            {   
                _context.Remove(PulledUser);
                _context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--------------------Already RSVP'd!--------------------");
            }
            // Redirect from a successful post request
            return RedirectToAction("Dashboard");
        }

        //---------------------------Redirects
        public IActionResult NotAuthorized(LoginUser user)
        {
                return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
