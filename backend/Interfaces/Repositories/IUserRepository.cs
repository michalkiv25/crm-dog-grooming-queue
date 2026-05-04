using DogQueueApi.Models;

namespace DogQueueApi.Interfaces.Repositories;

/// <summary>Persistence for users — called only from auth business logic (<see cref="Managers.AuthManager"/>).</summary>
public interface IUserRepository
{
    bool UsernameExists(string canonicalUsername);
    void Add(User user);
    void SaveChanges();
    User? FindByUsernameAndPassword(string canonicalUsername, string password);
}
