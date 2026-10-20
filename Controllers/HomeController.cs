
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using HospitalAppointmentSystem.Models;
using System.Net.Http.Json;

namespace HospitalAppointmentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        private List<Doctor> GetDoctors()
        {
            return _cache.GetOrCreate("DoctorsList", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);

                return new List<Doctor>
                {
                    new Doctor { DoctorId = 1, DoctorName = "Dr. Rahul Sharma", Specialization = "Cardiologist", Experience = 10, ConsultationFee = 500 },
                    new Doctor { DoctorId = 2, DoctorName = "Dr. Priya Patel", Specialization = "Dermatologist", Experience = 7, ConsultationFee = 400 },
                    new Doctor { DoctorId = 3, DoctorName = "Dr. Amit Shah", Specialization = "Neurologist", Experience = 12, ConsultationFee = 700 }
                };
            })!;
        }

        public IActionResult Index()
        {
            return View(GetDoctors());
        }

        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public IActionResult Details(int DoctorId)
        {
            var doctor = GetDoctors().FirstOrDefault(d => d.DoctorId == DoctorId);

            if (doctor == null)
                return NotFound();

            ViewBag.TotalCharge = doctor.CalculateTotalCharge();
            return View(doctor);
        }

        public IActionResult Appointment(int DoctorId)
        {
            var doctor = GetDoctors().FirstOrDefault(d => d.DoctorId == DoctorId);

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        [HttpPost]
        public IActionResult BookAppointment(int DoctorId, string PatientName)
        {
            Response.Cookies.Append("PatientName", PatientName ?? "");
            HttpContext.Session.SetString("Specialization",
                GetDoctors().FirstOrDefault(d => d.DoctorId == DoctorId)?.Specialization ?? "");

            ViewBag.Message = "Appointment request submitted successfully!";
            ViewBag.PatientName = PatientName;
            return View("Confirmation");
        }

        public async Task<IActionResult> Posts()
        {
            var client = _httpClientFactory.CreateClient();
            var posts = await client.GetFromJsonAsync<List<Post>>(
                "https://jsonplaceholder.typicode.com/posts");

            return View(posts ?? new List<Post>());
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
