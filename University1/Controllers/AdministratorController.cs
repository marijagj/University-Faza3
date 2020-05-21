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
    public class AdministratorController : Controller
    {
        private readonly University1Context _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdministratorController(University1Context context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            webHostEnvironment = hostEnvironment;
        }
        // GET: Administrator
        public ActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CourseIndex(string courseProgramme, string searchString, string courseSemester)
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
            courses = courses.Include(m => m.FirstTeacher).Include(m => m.SecondTeacher)
                             .Include(m => m.Students).ThenInclude(m => m.Student);
            var CoursesVM = new CourseSemesterProgrammeViewModel
            {
                Programmes = new SelectList(await programmeQuery.ToListAsync()),
                Semesters = new SelectList(await semesterQuery.ToListAsync()),
                Courses = await courses.ToListAsync()
            };
            return View(CoursesVM); ;
        }

        // GET: Administrator/CourseDetails/5
        public async Task<IActionResult> CourseDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .Include(c => c.Students).ThenInclude(c => c.Student)
                .FirstOrDefaultAsync(m => m.id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult CourseCreate()
        {
            ViewData["FirstTeacherId"] = new SelectList(_context.Teacher, "Id", "FullName", null);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teacher, "Id", "FullName", null);
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseCreate([Bind("id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CourseIndex));
            }
            ViewData["FirstTeacherId"] = new SelectList(_context.Teacher, "Id", "FullName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teacher, "Id", "FullName", course.SecondTeacherId);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> CourseEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = _context.Course.Where(m => m.id == id).Include(m => m.Students).First();
            if (course == null)
            {
                return NotFound();
            }
            CourseStudentsVM viewmodel = new CourseStudentsVM
            {
               StudentList = new MultiSelectList(_context.Student.OrderBy(s => s.FirstName), "Id", "FirstName"),
               SelectedStudents = course.Students.Select(sa => (int)sa.StudentId).ToList()
           };
            ViewData["Idd"] = id;
            ViewData["CourseTitle"] = _context.Course.Where(c =>c.id==id).Select(c=>c.Title).FirstOrDefault();
            return View(viewmodel);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseEdit(int id, CourseStudentsVM viewmodel)
        {
            if (id != viewmodel.Enrollments.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                {
                    IEnumerable<int> liststudents = viewmodel.SelectedStudents;
                    IEnumerable<int> existstudents = _context.Enrollment.Where(s => liststudents.Contains((int)s.StudentId) && s.CourseId == id).Select(s => (int)s.StudentId);
                    IEnumerable<int> newstudents = liststudents.Where(s => !existstudents.Contains(s));
                    foreach (int studentId in newstudents) _context.Enrollment.Add(new Enrollment { StudentId = studentId, CourseId = id, Year = viewmodel.Enrollments.Year, Semester = viewmodel.Enrollments.Semester });

                    await _context.SaveChangesAsync();
                }
                //catch (DbUpdateConcurrencyException)
                //{
                //    if (!CourseExists(viewmodel.Course.id))
                //    {
                //        return NotFound();
                //    }
                //    else
                //    {
                //        throw;
                //    }
                //}
                return RedirectToAction(nameof(CourseIndex));
            }
            //ViewData["FirstTeacherId"] = new SelectList(_context.Teacher, "Id", "FirstName", viewmodel.Course.FirstTeacherId);
            //ViewData["SecondTeacherId"] = new SelectList(_context.Teacher, "Id", "FirstName", viewmodel.Course.SecondTeacherId);
            return View(viewmodel);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> CourseDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .Include(c => c.Students).ThenInclude(c => c.Student)
                .FirstOrDefaultAsync(m => m.id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("CourseDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseDeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CourseIndex));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.id == id);
        }
        public async Task<IActionResult> StudentIndex(string StudentName, string StudentIDs)
        {
            IQueryable<Student> students = _context.Student.AsQueryable();
            IQueryable<string> IDQuery = _context.Student.OrderBy(m => m.StudentId).Select(m => m.StudentId).Distinct();

            if (!string.IsNullOrEmpty(StudentName))
            { students = students.Where(s => s.FirstName.Contains(StudentName) || s.LastName.Contains(StudentName)); }

            if (!string.IsNullOrEmpty(StudentIDs))
            { students = students.Where(x => x.StudentId == StudentIDs); }

            //students = students.Include(s => s.Courses).ThenInclude(s => s.Course);

            var studentnameVM = new StudentNameIdViewModel
            {
                IDs = new SelectList(await IDQuery.ToListAsync()),
                Students = await students.ToListAsync()
            };
            return View(studentnameVM); ;
        }

        // GET: Administrator/StudentDetails/5
        public async Task<IActionResult> StudentDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult StudentCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentCreate(StudentVM model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(model);

                Student teacher = new Student
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StudentId = model.StudentId,
                    EnrollmentDate = model.EnrollmentDate,
                    AcquiredCredits = model.AcquiredCredits,
                    CurrentSemester = model.CurrentSemester,
                    EducationLevel = model.EducationLevel,
                    ProfilePicture = uniqueFileName,
                };
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StudentIndex));
            }
            return View();
        }


        private string UploadedFile(StudentVM model)
        {
            string uniqueFileName = null;

            if (model.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfilePicture.CopyTo(fileStream);
                }
            }


            return uniqueFileName;
        }

        // GET: Administrator/StudentEdit/5
        public async Task<IActionResult> StudentEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentEdit(int id, [Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(StudentIndex));
            }
            return View(student);
        }

        // GET: Administrator/StudentDelete/5
        public async Task<IActionResult> StudentDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("StudentDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentDeleteConfirmed(int id)
        {
            var student = await _context.Student.FindAsync(id);
            _context.Student.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(StudentIndex));
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
        public async Task<IActionResult> TeacherIndex(string TeacherRank, string TeacherDegree, string TeacherName)
        {
            IQueryable<Teacher> teachers = _context.Teacher.AsQueryable();
            IQueryable<string> RankQuery = _context.Teacher.OrderBy(m => m.AcademicRank).Select(m => m.AcademicRank).Distinct();
            IQueryable<string> DegreeQuery = _context.Teacher.OrderBy(m => m.Degree).Select(m => m.Degree).Distinct();
            if (!string.IsNullOrEmpty(TeacherName))
            { teachers = teachers.Where(s => s.FirstName.Contains(TeacherName) || s.LastName.Contains(TeacherName)); }
            if (!string.IsNullOrEmpty(TeacherDegree))
            { teachers = teachers.Where(x => x.Degree == TeacherDegree); }
            if (!string.IsNullOrEmpty(TeacherRank))
            { teachers = teachers.Where(x => x.AcademicRank == TeacherRank); }

            teachers = teachers.Include(t => t.FTCourses)
                             .Include(t => t.STCourses);
            var teachernameVM = new TeacherNameDegreeViewModel
            {
                Degrees = new SelectList(await DegreeQuery.ToListAsync()),
                Ranks = new SelectList(await RankQuery.ToListAsync()),
                Teachers = await teachers.ToListAsync()
            };
            return View(teachernameVM);
        }

        // GET: Administrator/TeacherDetails/5
        public async Task<IActionResult> TeacherDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .Include(t => t.FTCourses)
                .Include(t => t.STCourses)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Administrator/TeacherCreate
        public IActionResult TeacherCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherCreate(TeacherVM model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(model);

                Teacher teacher = new Teacher
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Degree = model.Degree,
                    AcademicRank = model.AcademicRank,
                    OfficeNumber = model.OfficeNumber,
                    HireDate = model.HireDate,
                    ProfilePicture = uniqueFileName,
                };
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(TeacherIndex));
            }
            return View();
        }
    

        private string UploadedFile(TeacherVM model)
        {
            string uniqueFileName = null;

            if (model.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfilePicture.CopyTo(fileStream);
                }
            }
           
        
            return uniqueFileName;
        }
    

    // GET: Teachers/Edit/5
    public async Task<IActionResult> TeacherEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherEdit(int id, TeacherNameDegreeViewModel teacher)
        {
            if (id != teacher.TeacherVMs[0].Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.TeacherVMs[0].Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(TeacherIndex));
            }
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> TeacherDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher.Include(t => t.FTCourses)
                                                .Include(t => t.STCourses)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("TeacherDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherDeleteConfirmed(int id)
        {
            var teacher = await _context.Teacher.FindAsync(id);
            _context.Teacher.Remove(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(TeacherIndex));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teacher.Any(e => e.Id == id);
        }
        public async Task<IActionResult> EnrollmentIndex()
        {
            var university1Context = _context.Enrollment.Include(e => e.Course).Include(e => e.Student);
            return View(await university1Context.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> EnrollmentDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult EnrollmentAdd()
        {
            ViewData["CourseId"] = new SelectList(_context.Course, "id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollmentAdd([Bind("Id,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate,StudentId,CourseId")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EnrollmentIndex));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> EnrollmentEdit(int? id)
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
            ViewData["CourseId"] = new SelectList(_context.Course, "id", "id", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "Id", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollmentEdit(int id, [Bind("Id,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate,StudentId,CourseId")] Enrollment enrollment)
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
                return RedirectToAction(nameof(EnrollmentIndex));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> EnrollmentDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("EnrollmentDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollmentDeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);
            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(EnrollmentIndex));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollment.Any(e => e.Id == id);
        }

    }
    }

    


    



    




