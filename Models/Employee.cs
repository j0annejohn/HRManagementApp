using System.ComponentModel.DataAnnotations;

namespace HRManagementApp.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }

        [Display(Name = "Attendance Date/Time")]
        public DateTime ClockInTime { get; set; } = DateTime.Now; 

        public string AttendanceStatus { get; set; } // e.g., Present, Absent, Late
    }
}