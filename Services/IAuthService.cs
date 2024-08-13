using ReservationApi.Dtos;
using ReservationApi.Models;
using User = ReservationApi.Dtos.User;

namespace ReservationApi.Services;

public interface IAuthService
{
    public User Login(string email, string password);
    public User Register(string firstName, string lastName, string email, string password);
}