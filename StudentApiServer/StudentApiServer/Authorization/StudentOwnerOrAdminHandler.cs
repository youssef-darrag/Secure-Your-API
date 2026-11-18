using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace StudentApiServer.Authorization
{
    public class StudentOwnerOrAdminHandler : AuthorizationHandler<StudentOwnerOrAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOwnerOrAdminRequirement requirement, int studentId)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement); // If the user is an admin, they automatically succeed the requirement.
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier); // Retrieves the user ID from the JWT claims.
            if (int.TryParse(userId, out int authenticatedStudentId) && authenticatedStudentId == studentId)
            {
                context.Succeed(requirement); // If the user is the owner of the student data, they succeed the requirement.
            }
            return Task.CompletedTask;
        }
    }
}
