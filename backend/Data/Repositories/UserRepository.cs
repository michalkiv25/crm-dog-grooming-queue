using DogQueueApi.Interfaces.Repositories;
using DogQueueApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DogQueueApi.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public bool UsernameExists(string canonicalUsername) =>
        _db.Users.Any(u => u.Username == canonicalUsername);

    public void Add(User user) => _db.Users.Add(user);

    public void SaveChanges() => _db.SaveChanges();

    public User? FindByUsernameAndPassword(string canonicalUsername, string password) =>
        _db.Users.FirstOrDefault(u => u.Username == canonicalUsername && u.Password == password);
}
