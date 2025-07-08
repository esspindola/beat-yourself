using System;

namespace backend.DTOs.Account
{
  public class RegisterRequest
  {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
  }

}