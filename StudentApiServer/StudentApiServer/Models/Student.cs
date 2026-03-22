namespace StudentApiServer.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int Age { get; set; }
        public int Grade { get; set; }

        // Authentication-related fields
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
