using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagementApp.Data;
using HRManagementApp.Models;
using System.Text; // Required for CSV generation
using System.Net.Http; // Required for API Fetch
using System.Text.Json; // Required for API JSON parsing

namespace HRManagementApp.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly HRManagementAppContext _context;

        public EmployeesController(HRManagementAppContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. UPDATED INDEX: CALCULATES DASHBOARD STATS
        // ==========================================
        public async Task<IActionResult> Index(string searchString)
        {
            var employeesQuery = from e in _context.Employee select e;

            // Fetch all for dashboard calculations
            var allData = await employeesQuery.ToListAsync();

            // Store counts in ViewBag for the UI cards
            ViewBag.TotalCount = allData.Count;

            // Logic: Count only those who clocked in TODAY with specific statuses
            ViewBag.PresentToday = allData.Count(e => e.AttendanceStatus == "Present" && e.ClockInTime.Date == DateTime.Today);
            ViewBag.LateToday = allData.Count(e => e.AttendanceStatus == "Late" && e.ClockInTime.Date == DateTime.Today);

            // Logic: Absent means they are marked absent OR they haven't clocked in at all today
            ViewBag.AbsentToday = allData.Count(e => e.AttendanceStatus == "Absent" || (e.ClockInTime.Date != DateTime.Today));

            // Search Filter
            if (!String.IsNullOrEmpty(searchString))
            {
                employeesQuery = employeesQuery.Where(s => s.Name.Contains(searchString));
            }

            return View(await employeesQuery.ToListAsync());
        }

        // ==========================================
        // 2. NEW: DOWNLOAD ATTENDANCE REPORT (CSV)
        // ==========================================
        public async Task<IActionResult> DownloadReport()
        {
            var data = await _context.Employee.ToListAsync();
            var builder = new StringBuilder();

            // CSV Headers
            builder.AppendLine("ID,Name,Email,Department,ClockInTime,Status");

            foreach (var emp in data)
            {
                // Clean the data for CSV (removing commas from names if they exist)
                var safeName = emp.Name?.Replace(",", "");
                builder.AppendLine($"{emp.Id},{safeName},{emp.Email},{emp.Department},{emp.ClockInTime:yyyy-MM-dd HH:mm},{emp.AttendanceStatus}");
            }

            // Returns the data as a downloadable file
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"Attendance_Report_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ==========================================
        // 3. NEW: API INTEGRATION (RANDOMIZED FETCH)
        // ==========================================
        public async Task<IActionResult> FetchFromApi()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // RANDOMIZER: Pick a random ID between 1 and 10 so it's not always Leanne Graham
                    int randomUserId = new Random().Next(1, 11);

                    // Calling the public API with the random ID
                    var response = await client.GetStringAsync($"https://jsonplaceholder.typicode.com/users/{randomUserId}");
                    using var doc = JsonDocument.Parse(response);
                    var root = doc.RootElement;

                    var newEmployee = new Employee
                    {
                        Name = root.GetProperty("name").GetString(),
                        Email = root.GetProperty("email").GetString(),
                        Department = "Remote/External",
                        ClockInTime = DateTime.Now,
                        AttendanceStatus = "Present"
                    };

                    _context.Add(newEmployee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Successfully imported {newEmployee.Name} from external API!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Could not fetch API data. Please check your internet connection.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // --- EXISTING CRUD METHODS (Kept intact) ---

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employee.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Department,ClockInTime,AttendanceStatus")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Department,ClockInTime,AttendanceStatus")] Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employee.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            if (employee != null) _context.Employee.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }
    }
}
