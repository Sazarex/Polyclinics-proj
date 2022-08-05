using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models
{
    public class DocViewModel: IBaseModel
    {

        public int Id { get; set; }
        public string FIO { get; set; }
        public ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();// Specialization
        public ICollection<Specialization> OtherSpecializations { get; set; } = new List<Specialization>();
        public ICollection<Polyclinic> Polyclinics { get; set; } = new List<Polyclinic>();
        public ICollection<Polyclinic> OtherPolyclinics { get; set; } = new List<Polyclinic>();
        public int? Price { get; set; }
        public string? Phone { get; set; }
        public string? Photo { get; set; }
        public string? ShortDesc { get; set; }
        public string? FullDesc { get; set; }
        public IFormFile PhotoToUpload { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    }
}
