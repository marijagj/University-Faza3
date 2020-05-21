using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace University1.Models
{
    public class TeacherCourseViewModel
    {
        public IList<Course> Courses { get; set; }
        public string SearchString { get; set; }
    }
}
