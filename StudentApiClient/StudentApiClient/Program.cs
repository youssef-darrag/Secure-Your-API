using System.Net.Http.Json;

namespace StudentApiClient
{
    public class Program
    {
        static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            httpClient.BaseAddress = new Uri("http://localhost:5003/api/Students/"); // Set this to the correct URI of your API

            // this will show all students 
            await GetAll();

            // this will show passed students only
            await GetPassed();

            // this will show the calculated average for students grades
            await GetAverageGrade();

            // this will show the info for student 1
            await GetById(1); // Example: Get student with ID 1

            // this will show the info for student 20 and show not found because 20 is not there
            await GetById(20); // Example: Get student with ID 20

            // this will add new student
            var newStudent = new Student { Name = "Mazen Abdullah", Age = 20, Grade = 85 };
            await Create(newStudent); // Example: Add a new student

            // this will show all students after adding new one
            await GetAll();

            // this will delete student 1
            await Delete(1); // Example: Delete student with ID 1

            // this will show all students after deleting student 1
            await GetAll();

            // this will Update student 2
            await Update(2, new Student { Name = "Salma", Age = 22, Grade = 90 }); // Example: Update student with ID 2

            // this will show all students after Updating student 2
            await GetAll();
        }

        static async Task GetAll()
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine("\nFetching all students...\n");

                var response = await httpClient.GetAsync("GetAll");
                if (response.IsSuccessStatusCode)
                {
                    var students = await response.Content.ReadFromJsonAsync<List<Student>>();
                    if (students != null && students.Count > 0)
                    {
                        foreach (var student in students)
                        {
                            Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("No students found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task GetPassed()
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine("\nFetching passed students...\n");

                var response = await httpClient.GetAsync("GetPassed");
                if (response.IsSuccessStatusCode)
                {
                    var passedStudents = await response.Content.ReadFromJsonAsync<List<Student>>();
                    if (passedStudents != null && passedStudents.Count > 0)
                    {
                        foreach (var student in passedStudents)
                        {
                            Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("No students passed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task GetAverageGrade()
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine("\nFetching average grade...\n");

                var response = await httpClient.GetAsync("GetAverageGrade");
                if (response.IsSuccessStatusCode)
                {
                    var averageGrade = await response.Content.ReadFromJsonAsync<double>();
                    Console.WriteLine($"Average Grade: {averageGrade}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("No students found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task GetById(int id)
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine($"\nFetching student with ID {id}...\n");

                var response = await httpClient.GetAsync($"GetById/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var student = await response.Content.ReadFromJsonAsync<Student>();
                    if (student != null)
                    {
                        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine($"Bad Request: Invalid student ID.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Not Found: Student with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task Create(Student student)
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine("\nAdding a new student...\n");

                var response = await httpClient.PostAsJsonAsync("Create", student);
                if (response.IsSuccessStatusCode)
                {
                    var addedStudent = await response.Content.ReadFromJsonAsync<Student>();
                    Console.WriteLine($"Added Student - ID: {addedStudent!.Id}, Name: {addedStudent.Name}, Age: {addedStudent.Age}, Grade: {addedStudent.Grade}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Bad Request: Invalid student data.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task Update(int id, Student updatedStudent)
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine($"\nUpdating student with ID {id}...\n");

                var response = await httpClient.PutAsJsonAsync($"Update/{id}", updatedStudent);
                if (response.IsSuccessStatusCode)
                {
                    var student = await response.Content.ReadFromJsonAsync<Student>();
                    Console.WriteLine($"Updated Student: ID: {student!.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Failed to update student: Invalid student data.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Student with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task Delete(int id)
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine($"\nDeleting student with ID {id}...\n");

                var response = await httpClient.DeleteAsync($"Delete/{id}");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Student with ID {id} has been deleted.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine($"Bad Request: Invalid student ID.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Not Found: Student with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int Age { get; set; }
        public int Grade { get; set; }
    }
}
