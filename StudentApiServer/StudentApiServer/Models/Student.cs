namespace StudentApiServer.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int Age { get; set; }
        public int Grade { get; set; }
    }
}
