using Microsoft.AspNetCore.Authorization;

namespace StudentApiServer.Authorization
{
    public class StudentOwnerOrAdminRequirement : IAuthorizationRequirement
    {
        // This class represents a custom authorization requirement that checks if the user is either an admin or the owner of the student data.
        // It implements the IAuthorizationRequirement interface, which is used by the ASP.NET Core authorization system to define custom requirements.
    }
}
