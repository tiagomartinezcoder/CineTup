using CineTup.Application.Abstractions.Infraestructure;
using CineTup.Application.Exceptions;
using CineTup.Domain.Entities;
using CineTup.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CineTup.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CineTupDbContext _context;

        public UserRepository(CineTupDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => (u as Client).Tickets)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(User user, string newRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (user is Client client)
                {
                    foreach (var ticket in client.Tickets)
                    {
                        ticket.ClientId = null;
                        ticket.IsAvailable = true;
                        ticket.PurchaseDate = null;
                    }

                    _context.Clients.Remove(client);
                }
                else if (user is Admin admin)
                {
                    _context.Admins.Remove(admin);
                }
                else if (user is SysAdmin sysAdmin)
                {
                    if (await _context.SysAdmins.CountAsync(x => !x.IsDeleted) <= 1)
                        throw new ValidationException("No se puede cambiar el rol del único SysAdmin.");

                    _context.SysAdmins.Remove(sysAdmin);
                }

                bool emailExists = newRole switch
                {
                    "Client" => await _context.Clients.AnyAsync(c => c.Email == user.Email && !c.IsDeleted),
                    "Admin" => await _context.Admins.AnyAsync(a => a.Email == user.Email && !a.IsDeleted),
                    "SysAdmin" => await _context.SysAdmins.AnyAsync(s => s.Email == user.Email && !s.IsDeleted),
                    _ => false
                };

                if (emailExists)
                    throw new ConflictException("Ya existe un usuario con ese email en el rol destino.");

                User newUser = newRole switch
                {
                    "Client" => new Client
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Password = user.Password,
                        AvatarUrl = user.AvatarUrl
                    },

                    "Admin" => new Admin
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Password = user.Password,
                        AvatarUrl = user.AvatarUrl
                    },

                    "SysAdmin" => new SysAdmin
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Password = user.Password,
                        AvatarUrl = user.AvatarUrl
                    },

                    _ => throw new ValidationException("Rol inválido.")
                };

                _context.Add(newUser);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}