// using Google.Apis.Auth;
// using Microsoft.AspNetCore.Mvc;
// using System.Threading.Tasks;

// [Route("api/[controller]")]
// [ApiController]
// public class GoogleAuthController : ControllerBase
// {
//     private readonly IConfiguration _config;
//     private readonly JWTService _jwtService;

//     public GoogleAuthController(IConfiguration config, JWTService jwtService)
//     {
//         _config = config;
//         _jwtService = jwtService;
//     }

//     [HttpPost("google-login")]
//     public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
//     {
//         try
//         {
//             var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings()
//             {
//                 Audience = new[] { _config["Authentication:Google:ClientId"] }
//             });

//             var email = payload.Email;
//             var name = payload.Name;

//             // TODO: Optionally check/create user in DB based on email

//             var jwt = _jwtService.GenerateTokenForGoogleUser(email, name);

//             return Ok(new { token = jwt });
//         }
//         catch (InvalidJwtException)
//         {
//             return Unauthorized("Invalid Google token");
//         }
//     }
// }
