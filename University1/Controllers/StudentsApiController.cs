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
    public class StudentsApiController : ControllerBase
    {
        private readonly University1Context _context;

        public StudentsApiController(University1Context context)
        {
            _context = context;
        }

        // GET: api/StudentsApi
        [HttpGet]
        public List<Student> GetStudent(string StudentName, string StudentIDs)
        {
            IQueryable<Student> students = _context.Student.AsQueryable();
            IQueryable<string> IDQuery = _context.Student.OrderBy(m => m.StudentId).Select(m => m.StudentId).Distinct();

            if (!string.IsNullOrEmpty(StudentName))
            { students = students.Where(s => s.FirstName.Contains(StudentName) || s.LastName.Contains(StudentName)); }

            if (!string.IsNullOrEmpty(StudentIDs))
            { students = students.Where(x => x.StudentId == StudentIDs); }
            students = students.Include(s => s.Courses).ThenInclude(s => s.Course);
            return students.ToList();
        }

        // GET: api/StudentsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Student.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // PUT: api/StudentsApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        // POST: api/StudentsApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            _context.Student.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.Id }, student);
        }

        // DELETE: api/StudentsApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Student>> DeleteStudent(int id)
        {
            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Student.Remove(student);
            await _context.SaveChangesAsync();

            return student;
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
        // GET: api/StudentsApi
        [HttpGet("{id}/GetCourses")]
        public async Task<IActionResult> GetCoursesOfStudents([FromRoute] int id)
        {
            var student = await _context.Student.FindAsync(id);
            if (student == null) { return NotFound(); }
            var enrollment = _context.Enrollment.Where(m => m.StudentId == id).ToList();
            List<Course> courses = new List<Course>();
            foreach (var course in enrollment)
            {
                Course newcourse = _context.Course.Where(m => m.id == course.CourseId).FirstOrDefault();
                newcourse.Students = null;
                courses.Add(newcourse);
            }
            return Ok(courses);
        }
    }
}
