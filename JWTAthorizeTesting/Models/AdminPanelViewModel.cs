using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models
{
    public class AdminPanelViewModel
    {
        public List<City> Cities { get; set; } = new List<City>();
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public List<Experience> Experiences { get; set; } = new List<Experience>();
        public List<Polyclinic> Polyclinics { get; set; } = new List<Polyclinic>();
        public List<Specialization> Specializations { get; set; } = new List<Specialization>();

        //Для сохранение фото
        public IFormFile Photo { get; set; }

    }
}
