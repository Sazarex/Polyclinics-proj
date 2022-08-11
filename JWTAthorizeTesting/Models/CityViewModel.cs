using JWTAthorizeTesting.Entities;
using System.ComponentModel.DataAnnotations;

namespace JWTAthorizeTesting.Models
{
    public class CityViewModel: IBaseModel
    {
        public int CityId { get; set; }

        [Required]
        public string Title { get; set; }

        public IList<Polyclinic>? OtherPolyclinics { get; set; } = new List<Polyclinic>();

        public IList<Polyclinic>? PolyclinicsOfCity { get; set; } = new List<Polyclinic>();

        public IList<City>? Cities { get; set; } = new List<City>();

        
        public PageViewModel? pageViewModel { get ; set ; }
    }
}
