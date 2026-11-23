using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using StudentApiServer.DataSimulation;
using StudentApiServer.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentApiServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")] // Applies the "AuthLimiter" policy to this endpoint, limiting the number of login attempts per IP address.
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // ✅ Capture caller IP once (used in all logs for tracing)
            // 📌 We store IP as a string and default to "unknown" to avoid null issues.
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Step 1: Find the student by email from the in-memory data store.
            // Email acts as the unique login identifier.
            var student = StudentDataSimulation.Students.FirstOrDefault(s => s.Email == request.Email);

            // ===============================
            // Failure Path #1: Email not found
            // ===============================
            // ✅ Security logging: record the failure safely
            // ✅ We log the Email + IP only (NO password, NO tokens).
            // 📌 This helps detect brute-force / credential stuffing attempts.
            if (student == null)
            {
                _logger.LogWarning("Failed login attempt (email not found). Email={Email}, IP={IP}", request.Email, ip);

                // Return generic message to avoid revealing whether email exists.
                return Unauthorized("Invalid credentials.");
            }

            // Step 2: Verify the provided password against the stored hash.
            // BCrypt handles hashing and salt internally.
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash);

            // ===============================
            // Failure Path #2: Password invalid
            // ===============================
            // ✅ Security logging: record repeated password failures
            // 📌 This helps detect brute-force attempts on known emails.
            if (!isValidPassword)
            {
                _logger.LogWarning("Failed login attempt (bad password). Email={Email}, IP={IP}", request.Email, ip);

                // Return generic message to avoid revealing which field is wrong.
                return Unauthorized("Invalid credentials.");
            }

            // Step 3: Create claims that represent the authenticated user's identity.
            // These claims will be embedded inside the JWT.
            var claims = new[]
            {
                // Unique identifier for the student.
                new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),

                // Student email address.
                new Claim(ClaimTypes.Email, student.Email),

                // Role (Student or Admin) used later for authorization.
                new Claim(ClaimTypes.Role, student.Role)
            };

            // Step 4: Create the symmetric security key used to sign the JWT.
            // This key must match the key used in JWT validation middleware.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));


            // Step 5: Define the signing credentials.
            // This specifies the algorithm used to sign the token.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Step 6: Create the JWT token.
            // The token includes issuer, audience, claims, expiration, and signature.
            var token = new JwtSecurityToken(
                issuer: "StudentApi",
                audience: "StudentApiUsers",
                claims: claims,
                //expires: DateTime.Now.AddMinutes(30),
                expires: DateTime.UtcNow.AddSeconds(10),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Create refresh token (random)
            var refreshToken = GenerateRefreshToken();

            // Store refresh token securely (hash + expiry + not revoked)
            student.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            student.RefreshTokenRevokedAt = null;

            // ===============================
            // Optional success log (low noise)
            // ===============================
            // ✅ Safe success log: user ID + email + IP only (NO tokens)
            // 📌 Useful for later investigations (timeline reconstruction).
            _logger.LogInformation("Successful login. UserId={UserId}, Email={Email}, IP={IP}", student.Id, student.Email, ip);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("AuthLimiter")] // Applies the "AuthLimiter" policy to this endpoint, limiting the number of refresh attempts per IP address.
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            // ✅ Capture caller IP once (used in all logs for tracing)
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var student = StudentDataSimulation.Students.FirstOrDefault(s => s.Email == request.Email);

            // ===============================
            // Failure Path #1: Email not found
            // ===============================
            // ✅ Safe log: Email + IP only
            // 📌 Helps detect refresh probing / abuse attempts.
            if (student == null)
            {
                _logger.LogWarning("Invalid refresh attempt (email not found). Email={Email}, IP={IP}", request.Email, ip);

                return Unauthorized("Invalid refresh request");
            }

            // ===============================
            // Failure Path #2: Token already revoked
            // ===============================
            // ✅ Safe log: UserId + Email + IP only
            // 📌 Indicates possible reuse of an old token (suspicious).
            if (student.RefreshTokenRevokedAt != null)
            {
                _logger.LogWarning("Refresh attempt using revoked token. UserId={UserId}, Email={Email}, IP={IP}", student.Id, student.Email, ip);

                return Unauthorized("Refresh token is revoked");
            }

            // ===============================
            // Failure Path #3: Token expired
            // ===============================
            // ✅ Safe log: UserId + Email + IP only
            // 📌 Expired refresh usage can be normal or automated retry — log helps visibility.
            if (student.RefreshTokenExpiresAt == null || student.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh attempt using expired token. UserId={UserId}, Email={Email}, IP={IP}", student.Id, student.Email, ip);

                return Unauthorized("Refresh token expired");
            }

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
            // ===============================
            // Failure Path #4: Invalid refresh token value
            // ===============================
            // ❌ Never log the raw refresh token
            // ✅ Only log outcome + identity data
            if (!refreshValid)
            {
                _logger.LogWarning("Invalid refresh token attempt. UserId={UserId}, Email={Email}, IP={IP}", student.Id, student.Email, ip);

                return Unauthorized("Invalid refresh token");
            }

            // Issue NEW access token (same claims & signing settings as login)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
                new Claim(ClaimTypes.Email, student.Email),
                new Claim(ClaimTypes.Role, student.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: "StudentApi",
                audience: "StudentApiUsers",
                claims: claims,
                //expires: DateTime.UtcNow.AddMinutes(30),
                expires: DateTime.UtcNow.AddSeconds(10),
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Rotation: replace refresh token
            var newRefreshToken = GenerateRefreshToken();
            student.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            student.RefreshTokenRevokedAt = null;

            // ✅ Optional low-noise success log (safe)
            _logger.LogInformation("Refresh succeeded. UserId={UserId}, Email={Email}, IP={IP}", student.Id, student.Email, ip);

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            var student = StudentDataSimulation.Students.FirstOrDefault(s => s.Email == request.Email);

            if (student == null)
                return Ok(); // Do not reveal if user exists

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
            if (!refreshValid)
                return Ok();

            student.RefreshTokenRevokedAt = DateTime.UtcNow;
            return Ok("Logged out successfully");
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}
