using AuthenticationServer.Data;
using AuthenticationServer.DTO;
using AuthenticationServer.Exceptions;
using AuthenticationServer.Middleware;
using AuthenticationServer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationServer.Services;

public class AuthService(ApplicationDbContext context, IPasswordHasher passwordHasher , IJwtService jwtService) : IAuthService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IJwtService _jwtService = jwtService;

    public async Task RegisterAsync(RegisterRequest request)
    {
        bool isUserExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (isUserExists)
        {
            throw new EmailAlreadyExistsException(request.Email);
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            // Temporary (we'll hash it next)
            Role = Role.User,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        try
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                throw new EmailAlreadyExistsException(request.Email);
            }

            throw;
        }
    }

    public async Task <LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Invalid email or password.");
        }
        // Generate a token (for simplicity, we'll just return a dummy token here)
        var token = _jwtService.GenerateToken(user)  ;
        return new LoginResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token
        };
    }
}