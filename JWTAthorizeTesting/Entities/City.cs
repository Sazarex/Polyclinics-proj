using System.ComponentModel.DataAnnotations;

namespace JWTAthorizeTesting.Entities
{
    public class City
    {
        public int CityId { get; set; }

        [Required]
        public string Title { get; set; }
        public ICollection<Polyclinic>? Polyclinics { get; set; } = new List<Polyclinic>();
    }
}
