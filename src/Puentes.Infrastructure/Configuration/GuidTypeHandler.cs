using System.Data;
using Dapper;

namespace Puentes.Infrastructure.Configuration;

public sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString("D").ToUpperInvariant();
    }

    public override Guid Parse(object value)
    {
        return Guid.Parse(value.ToString()!);
    }
}
