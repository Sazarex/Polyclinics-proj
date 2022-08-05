using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models
{
    public class SpecViewModel: IBaseModel
    {
        public int SpecializationId { get; set; }
        public string Title { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Doctor> OtherDoctors { get; set; } = new List<Doctor>();
        public ICollection<Specialization> Specializations{ get; set; } = new List<Specialization>();

    }
}
