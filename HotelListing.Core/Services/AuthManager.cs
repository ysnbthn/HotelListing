using HotelListing.Core.DTOs;
using HotelListing.Core.Models;
using HotelListing.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Core.Services
{
    public class AuthManager : IAuthManager
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private ApiUser _user;

        public AuthManager(UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> CreateToken()
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var token = GenerateTokenOptions(signingCredentials, claims);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            // tokena süre ekle
            var expiration = DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("lifetime").Value));

            var token = new JwtSecurityToken(
                issuer: jwtSettings.GetSection("Issuer").Value,
                claims: claims,
                expires : expiration,
                signingCredentials: signingCredentials
                );

            return token;
        }

        // claim için ayrı metod
        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>()
            {
                // ben bunu yapabilceğimi iddia ediyorum demek 
                new Claim(ClaimTypes.Name, _user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(_user);

            foreach(var role in roles)
            {
                // her rol için claim ekle
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        // credential almak için ayrı metod
        private SigningCredentials GetSigningCredentials()
        {
            // sistem yerine dosyadan sistem değeri al
            DotNetEnv.Env.Load();
            var key = Environment.GetEnvironmentVariable("KEY");
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        // kullanıcı doğrulama
        public async Task<bool> ValidateUser(LoginUserDTO userDTO)
        {
            // başka fonksiyonda kullanmak için yukarıdan çektik
            _user = await _userManager.FindByNameAsync(userDTO.Email);
            return (_user != null && await _userManager.CheckPasswordAsync(_user, userDTO.Password));
        }

        public async Task<string> CreateRefreshToken()
        {
            // userın tokenı iptal et
            await _userManager.RemoveAuthenticationTokenAsync(_user, "HotelListingApi", "RefreshToken");
            // yeni token yap
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, "HotelListingApi", "RefreshToken");
            var result = await _userManager.SetAuthenticationTokenAsync(_user, "HotelListingApi", "RefreshToken", newRefreshToken);
            return newRefreshToken;
        }

        public async Task<TokenRequest> VerifyRefreshToken(TokenRequest request)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new (); // eğer class adı belliyse böyle newleyebilirsin
            // tokenı c# objesi yap
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == ClaimTypes.Name)?.Value;
            // claimden userı bul
            _user = await _userManager.FindByNameAsync(username);
            try
            {
                // token geçerliyse yenile yoksa userı at tekrar login olmak zorunda kalsın
                var isValid = await _userManager.VerifyUserTokenAsync(_user, "HotelListingApi", "RefreshToken", request.RefreshToken);
                if (isValid)
                {
                    return new TokenRequest { Token = await CreateToken(), RefreshToken = await CreateRefreshToken() };
                }
                await _userManager.UpdateSecurityStampAsync(_user);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }
    }
}
