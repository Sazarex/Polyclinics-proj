namespace JWTAthorizeTesting.Entities
{
    public class Role
    {
        public int RoleId { get; set; }

        public string Title { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}
