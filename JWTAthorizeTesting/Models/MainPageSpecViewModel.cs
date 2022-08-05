﻿using JWTAthorizeTesting.Entities;

namespace JWTAthorizeTesting.Models
{
    public class MainPageSpecViewModel
    {
        public int SpecializationId { get; set; }
        public string Title { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();


        public City CityOfSpec{ get; set; } 
    }
}
