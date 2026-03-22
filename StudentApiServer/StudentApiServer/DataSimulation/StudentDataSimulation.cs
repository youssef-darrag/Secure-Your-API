using StudentApiServer.Models;

namespace StudentApiServer.DataSimulation
{
    public class StudentDataSimulation
    {
        // Static list of students, acting as an in-memory data store, I can change it later on to retrieve students from Database.
        public static readonly List<Student> Students = new List<Student>
        {
            // Initialize the list with some student objects.
            new Student
            {
                Id = 1,
                Name = "Ali Ahmed",
                Age = 20,
                Grade = 88
            },
            new Student
            {
                Id = 2,
                Name = "Fadi Mohammed",
                Age = 22,
                Grade = 77
            },
            new Student
            {
                Id = 3,
                Name = "Ola Jaber",
                Age = 21,
                Grade = 66
            },
            new Student
            {
                Id = 4,
                Name = "Alia Maher",
                Age = 19,
                Grade = 44
            }
        };
    }
}
