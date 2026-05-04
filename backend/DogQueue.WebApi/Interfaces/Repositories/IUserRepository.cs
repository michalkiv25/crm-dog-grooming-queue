using DogQueue.WebApi.Models;

namespace DogQueue.WebApi.Interfaces.Repositories;

public interface IUserRepository
{
    bool UsernameExists(string canonicalUsername);
    void Add(User user);
    void SaveChanges();
    User? FindByUsernameAndPassword(string canonicalUsername, string password);
}
