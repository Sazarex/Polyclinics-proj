using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models
{
    public class MainPageViewModel
    {
        public int CityId { get; set; }
        public string Title { get; set; }
        public ICollection<Polyclinic>? Polyclinics { get; set; } = new List<Polyclinic>();



        public ICollection<Doctor> DocsOfCity { get; set; } = new List<Doctor>();
        public ICollection<Specialization> AllSpecializations { get; set; } = new List<Specialization>();
        public ICollection<City> AllCities { get; set; } = new List<City>();
    }
}
