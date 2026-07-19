using Puentes.Infrastructure.Database;
using System.Data;

namespace Puentes.Infrastructure.Repositories;

public abstract class RepositoryBase(AccessDb accessDb)
{
     protected IDbConnection CreateConnection()
    {
        return accessDb.OpenConnection();
    }
}   