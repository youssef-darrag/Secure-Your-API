using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApiServer.DataSimulation;
using StudentApiServer.Models;
using System.Security.Claims;

namespace StudentApiServer.Controllers
{
    [Authorize] // Requires authentication for all endpoints in this controller, ensuring that only authorized users can access the student data.
    [Route("api/[controller]")] // Sets the route for this controller to "api/Students", based on the controller name.
    [ApiController] // Marks the class as a Web API controller with enhanced features.
    public class StudentsController : ControllerBase
    {
        [HttpGet("GetAll")] // Defines an HTTP GET endpoint at "api/Students/GetAll".
        [Authorize(Roles = "Admin")] // Restricts access to this endpoint to users with the "Admin" role, ensuring that only administrators can retrieve all student data.
        [ProducesResponseType(StatusCodes.Status200OK)] // Indicates that this endpoint can return a 200 OK response.
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indicates that this endpoint can return a 404 Not Found response.
        public ActionResult<IEnumerable<Student>> GetAll()
        {
            //StudentDataSimulation.Students.Clear(); // Clears the existing list of students to ensure fresh data for each request.

            if (StudentDataSimulation.Students.Count == 0)
                return NotFound("No students found."); // Returns a 404 Not Found response if there are no students.

            return Ok(StudentDataSimulation.Students); // Returns a 200 OK response with the list of students.
        }

        [HttpGet("GetPassed")] // Defines an HTTP GET endpoint at "api/Students/GetPassed".
        [AllowAnonymous] // Allows anonymous access to this endpoint, meaning it can be accessed without authentication.
        [ProducesResponseType(StatusCodes.Status200OK)] // Indicates that this endpoint can return a 200 OK response.
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indicates that this endpoint can return a 404 Not Found response.
        public ActionResult<IEnumerable<Student>> GetPassed()
        {
            var passedStudents = StudentDataSimulation.Students.Where(s => s.Grade >= 50).ToList(); // Filters students who have passed (grade >= 50).
            //passedStudents.Clear(); // Clears the list of passed students to ensure fresh data for each request.

            if (passedStudents.Count == 0)
                return NotFound("No students passed."); // Returns a 404 Not Found response if there are no students passed.

            return Ok(passedStudents); // Returns a 200 OK response with the list of passed students.
        }

        [HttpGet("GetAverageGrade")] // Defines an HTTP GET endpoint at "api/Students/GetAverageGrade".
        [AllowAnonymous] // Allows anonymous access to this endpoint, meaning it can be accessed without authentication.
        [ProducesResponseType(StatusCodes.Status200OK)] // Indicates that this endpoint can return a 200 OK response.
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indicates that this endpoint can return a 404 Not Found response.
        public ActionResult<double> GetAverageGrade()
        {
            //StudentDataSimulation.Students.Clear(); // Clears the existing list of students to ensure fresh data for each request.

            if (StudentDataSimulation.Students.Count == 0)
                return NotFound("No students found."); // Returns a 404 Not Found response if there are no students.

            var averageGrade = StudentDataSimulation.Students.Average(s => s.Grade); // Calculates the average grade of all students.
            return Ok(averageGrade); // Returns a 200 OK response with the average grade.
        }

        [HttpGet("GetById/{id}")] // Defines an HTTP GET endpoint at "api/Students/GetById/{id}".
        [ProducesResponseType(StatusCodes.Status200OK)] // Indicates that this endpoint can return a 200 OK response.
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Indicates that this endpoint can return a 400 Bad Request response.
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indicates that this endpoint can return a 404 Not Found response.
        public async Task<ActionResult<Student>> GetById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 1)
                return BadRequest("Invalid student ID."); // Returns a 400 Bad Request response if the provided ID is invalid.

            var student = StudentDataSimulation.Students.FirstOrDefault(s => s.Id == id); // Finds a student by their ID.

            if (student == null)
                return NotFound($"Student with ID {id} not found."); // Returns a 404 Not Found response if the student is not found.

            var authResult = await authorizationService.AuthorizeAsync(User, id, "StudentOwnerOrAdmin"); // Checks if the user is authorized to access this student's data based on the custom requirement.

            if (!authResult.Succeeded)
                return Forbid("You are not authorized to access this student's data."); // Returns a 403 Forbidden response if the user is not authorized.

            return Ok(student); // Returns a 200 OK response with the student data.
        }

        [HttpPost("Create")] // Defines an HTTP POST endpoint at "api/Students/Create".
        [Authorize(Roles = "Admin")] // Restricts access to this endpoint to users with the "Admin" role, ensuring that only administrators can create new student records.
        [ProducesResponseType(StatusCodes.Status201Created)] // Indicates that this endpoint can return a 201 Created response.
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Indicates that this endpoint can return a 400 Bad Request response.
        public ActionResult<Student> Create(Student student)
        {
            if (student == null || string.IsNullOrEmpty(student.Name) || student.Age < 0 || student.Grade < 0)
                return BadRequest("Invalid student data."); // Returns a 400 Bad Request response if the student data is invalid.

            student.Id = StudentDataSimulation.Students.Count > 0 ? StudentDataSimulation.Students.Max(s => s.Id) + 1 : 1; // Assigns a new ID to the student based on the existing students.

            StudentDataSimulation.Students.Add(student); // Adds the new student to the list.

            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student); // Returns a 201 Created response with the location of the newly created student.
        }

        [HttpPut("Update/{id}")] // Defines an HTTP PUT endpoint at "api/Students/Update/{id}".
        [Authorize(Roles = "Admin")] // Restricts access to this endpoint to users with the "Admin" role, ensuring that only administrators can update student records.
        [ProducesResponseType(StatusCodes.Status200OK)] // Indicates that this endpoint can return a 200 OK response.
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Indicates that this endpoint can return a 400 Bad Request response.
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indicates that this endpoint can return a 404 Not Found response.
        public ActionResult<Student> Update(int id, Student updatedStudent)
        {
            if (id < 1 || updatedStudent == null || string.IsNullOrEmpty(updatedStudent.Name) || updatedStudent.Age < 0 || updatedStudent.Grade < 0)
                return BadRequest("Invalid student data."); // Returns a 400 Bad Request response if the updated student data is invalid.

            var student = StudentDataSimulation.Students.FirstOrDefault(s => s.Id == id); // Finds a student by their ID.
            if (student == null)
                return NotFound($"Student with ID {id} not found."); // Returns a 404 Not Found response if the student is not found.

            student.Name = updatedStudent.Name; // Updates the student's name.
            student.Age = updatedStudent.Age; // Updates the student's age.
            student.Grade = updatedStudent.Grade; // Updates the student's grade.

            return Ok(student); // Returns a 200 OK response with the updated student data.
        }

        [HttpDelete("Delete/{id}")] // Defines an HTTP DELETE endpoint at "api/Students/Delete/{id}".
        [Authorize(Roles = "Admin")] // Restricts access to this endpoint to users with the "Admin" role, ensuring that only administrators can delete student records.
        [ProducesResponseType(StatusCodes.Status200OK)] // Indicates that this endpoint can return a 200 OK response.
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Indicates that this endpoint can return a 400 Bad Request response.
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indicates that this endpoint can return a 404 Not Found response.
        public ActionResult Delete(int id)
        {
            if (id < 1)
                return BadRequest("Invalid student ID."); // Returns a 400 Bad Request response if the provided ID is invalid.

            var student = StudentDataSimulation.Students.FirstOrDefault(s => s.Id == id); // Finds a student by their ID.
            if (student == null)
                return NotFound($"Student with ID {id} not found."); // Returns a 404 Not Found response if the student is not found.

            StudentDataSimulation.Students.Remove(student); // Removes the student from the list.
            return Ok($"Student with ID {id} has been deleted."); // Returns a 200 OK response confirming the deletion of the student.
        }
    }
}
