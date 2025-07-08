using System;

namespace backend.DTOs.Account
{
  public class LoginRequest
  { 
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
  }
}