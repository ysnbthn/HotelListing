﻿
using HotelListing.Core.DTOs;
using HotelListing.Core.Models;

namespace HotelListing.Core.Services
{
    public interface IAuthManager
    {
        Task<bool> ValidateUser(LoginUserDTO userDTO);
        Task<string> CreateToken();
        Task<string> CreateRefreshToken();
        Task<TokenRequest> VerifyRefreshToken(TokenRequest request);
    }
}
