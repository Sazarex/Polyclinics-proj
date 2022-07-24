namespace JWTAthorizeTesting.Entities
{
    public class Polyclinic
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public int? CityId { get; set; }
        public City? City { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public string? Photo { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
