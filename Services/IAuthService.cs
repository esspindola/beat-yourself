using backend.DTOs.Account;
using System.Threading.Tasks;

namespace backend.Services
{
  public class AuthServiceResult<T>
  {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
  }

  public interface IAuthService
  {
    Task<AuthServiceResult<string>> RegisterAsync(RegisterRequest request);
    Task<AuthServiceResult<string>> LoginAsync(LoginRequest request);
  }
}