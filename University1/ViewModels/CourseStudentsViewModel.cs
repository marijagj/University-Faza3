using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace University1.Models
{
    public class CourseStudentsViewModel
    {
        public IList<Course> Courses { get; set; }
        public List<int> SelectedStudents { get; set; }
        public IEnumerable<SelectListItem> StudentList { get; set; }
        public IList<Enrollment> Enrollments { get; set; }
        public SelectList Years { get; set; }
        public string EnrollmentYear { get; set; }
    }
}
