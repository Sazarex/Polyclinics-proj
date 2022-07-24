namespace JWTAthorizeTesting.Entities
{
    public class Specialization
    {
        public int SpecializationId { get; set; }
        public string Title { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
