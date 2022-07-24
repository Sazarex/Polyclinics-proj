namespace JWTAthorizeTesting.Entities
{
    public class Experience
    {
        public int ExperienceId { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public DateTime BeginWork { get; set; }
    }
}
