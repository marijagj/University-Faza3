using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University1.Data;
using University1.Models;

namespace University1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesApiController : ControllerBase
    {
        private readonly University1Context _context;

        public CoursesApiController(University1Context context)
        {
            _context = context;
        }

        // GET: api/CoursesApi
        [HttpGet]
        public List<Course> GetCourse(string courseProgramme, string searchString, string courseSemester)
        {
            IQueryable<Course> courses = _context.Course.AsQueryable();
            IQueryable<string> programmeQuery = _context.Course.OrderBy(m => m.Programme).Select(m => m.Programme).Distinct();
            IQueryable<int> semesterQuery = _context.Course.OrderBy(m => m.Semester).Select(m => m.Semester).Distinct();
            if (!string.IsNullOrEmpty(searchString))
            { courses = courses.Where(s => s.Title.Contains(searchString)); }
            if (!string.IsNullOrEmpty(courseProgramme))
            { courses = courses.Where(x => x.Programme == courseProgramme); }
            if (!string.IsNullOrEmpty(courseSemester))
            { courses = courses.Where(x => x.Programme == courseSemester); }
            // courses = courses.Include(m => m.FirstTeacher).Include(m => m.SecondTeacher);
            //.Include(m => m.Students).ThenInclude(m => m.Student);
            return courses.ToList();
        }

        // GET: api/CoursesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Course.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        // PUT: api/CoursesApi/5-NE FUNKCIONIRA NA POSTMAN PORADI VIEW MODELOT
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutCourse(int id, CourseStudentsVM viewmodel)
        //{
        //    if (id !=viewmodel.Enrollments.CourseId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(viewmodel.Enrollments.CourseId).State = EntityState.Modified;

        //    try
        //    {
        //        IEnumerable<int> liststudents = viewmodel.SelectedStudents;
        //        IEnumerable<int> existstudents = _context.Enrollment.Where(s => liststudents.Contains((int)s.StudentId) && s.CourseId == id).Select(s => (int)s.StudentId);
        //        IEnumerable<int> newstudents = liststudents.Where(s => !existstudents.Contains(s));
        //        foreach (int studentId in newstudents) _context.Enrollment.Add(new Enrollment { StudentId = studentId, CourseId = id, Year = viewmodel.Enrollments.Year, Semester = viewmodel.Enrollments.Semester });

        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!CourseExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/CoursesApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(Course course)
        {
            _context.Course.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourse", new { id = course.id }, course);
        }

        // DELETE: api/CoursesApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Course>> DeleteCourse(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Course.Remove(course);
            await _context.SaveChangesAsync();

            return course;
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.id == id);
        }
        // GET: api/CoursesApi
        [HttpGet("{id}/GetStudents")]
        public async Task<IActionResult> GetStudentsInCourse([FromRoute] int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null) { return NotFound(); }
            var enrollment = _context.Enrollment.Where(m => m.CourseId == id).ToList();
            List<Student> students = new List<Student>();
            foreach (var actor in enrollment)
            {
                Student newstudent = _context.Student.Where(m => m.Id == actor.StudentId).FirstOrDefault();
                newstudent.Courses = null;
                students.Add(newstudent);
            }
            return Ok(students);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, Course course)
        {
            if (id != course.id)
            {
                return BadRequest();
            }

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
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
    }
}
