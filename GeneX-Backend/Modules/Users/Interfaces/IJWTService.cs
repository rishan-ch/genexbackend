// File: IJWTService.cs
using GeneX_Backend.Modules.Users.Entities;

namespace GeneX_Backend.Modules.Users.Interfaces
{
    public interface IJWTService
    {
        string GenerateToken(UserEntity user, IList<string> roles);
        string GenerateTokenForGoogleUser(string email, string name);
    }
}
