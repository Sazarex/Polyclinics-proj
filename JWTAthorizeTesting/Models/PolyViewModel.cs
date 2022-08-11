using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models
{
    public class PolyViewModel: IBaseModel
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public int? CityId { get; set; }
        public City? City { get; set; }
        public string? Adress { get; set; }
        public string? Phone { get; set; }
        public string? Photo { get; set; }
        public IFormFile PhotoToUpload { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Doctor> OtherDoctors { get; set; } = new List<Doctor>();
        public ICollection<Polyclinic> Polyclinics { get; set; } = new List<Polyclinic>();
        public PageViewModel pageViewModel { get ; set ; }
    }
}
