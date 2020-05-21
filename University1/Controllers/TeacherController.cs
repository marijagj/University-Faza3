using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using University1.Data;
using University1.Models;

namespace University1.Controllers
{
    public class TeacherController : Controller
    {

        private readonly University1Context _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public TeacherController(University1Context context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            webHostEnvironment = hostEnvironment;
        }

        // GET: Teacher/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Teacher/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Teacher/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Teacher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Teacher/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Teacher/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public async Task<IActionResult> Index(string searchString)
        {
            IQueryable<Course> courses = _context.Course.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            { courses = courses.Where(s => s.FirstTeacher.FirstName.Contains(searchString) || s.SecondTeacher.FirstName.Contains(searchString)); }
            courses = courses.Include(m => m.FirstTeacher).Include(m => m.SecondTeacher)
                             .Include(m => m.Students).ThenInclude(m => m.Student);
            var teachercourseVM = new TeacherCourseViewModel
            {
                Courses = await courses.ToListAsync()
            };
            return View(teachercourseVM); ;
        }
        
        public async Task<IActionResult> GetStudentsByCourse(int id, string enrollmentYear )
        {
            IQueryable<int> year = _context.Enrollment.OrderBy(m => (int)m.Year).Select(m => (int)m.Year).Distinct();
            IQueryable<Course> courses = _context.Course.AsQueryable();
            IQueryable<Enrollment> enrollments = _context.Enrollment.Where(m => m.CourseId == id);
            if (!string.IsNullOrEmpty(enrollmentYear))
            { enrollments = enrollments.Where(x => x.Year.ToString() == enrollmentYear); }
            courses = courses.Where(m => m.id == id)
                             .Include(m => m.Students).ThenInclude(m => m.Student);
            var viewModel = new CourseStudentsViewModel
            {
                Years = new SelectList(await year.ToListAsync()),
                Courses = await courses.ToListAsync(),
                Enrollments = await enrollments.ToListAsync()
            };

            return View(viewModel);


        }
        public IActionResult TeacherName()
        {
            return View(); ;
        }
        public async Task<IActionResult> Enrollment(string searchString)
        {
            IQueryable<Enrollment> enrollments = _context.Enrollment.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            { enrollments = enrollments.Where(s => s.Course.FirstTeacher.FirstName.Contains(searchString) || s.Course.SecondTeacher.FirstName.Contains(searchString)); }
            enrollments = enrollments.Include(m => m.Course).Include(m => m.Student);
            var teachercourseVM = new CourseStudentsViewModel
            {
                Enrollments = await enrollments.ToListAsync()
            };
            return View(teachercourseVM); 
        }
        public async Task<IActionResult> TeacherEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            if (enrollment.FinishDate != null)
            {
                return NotFound("This is not an Active Student");
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "id", "id", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "Id", enrollment.StudentId);
            return View(enrollment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherEdit(int id, [Bind("Id,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate,StudentId,CourseId")] Enrollment enrollment)
        {  
     
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Enrollment));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }

        private bool EnrollmentExists(int id)
        {
            throw new NotImplementedException();
        }
        public IActionResult TeacherEnrollment()
        {
            return View(); ;
        }
    }
}
