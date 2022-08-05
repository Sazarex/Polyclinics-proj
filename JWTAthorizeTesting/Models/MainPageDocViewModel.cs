using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models
{
    public class MainPageDocViewModel
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();// Specialization
        public ICollection<Polyclinic> Polyclinics { get; set; } = new List<Polyclinic>();
        public int? Price { get; set; }
        public string? Phone { get; set; }
        public string? Photo { get; set; }
        public string? ShortDesc { get; set; }
        public string? FullDesc { get; set; }
    }
}
