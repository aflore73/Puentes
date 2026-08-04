using System.Data;
using System.Globalization;
using Dapper;

namespace Puentes.Infrastructure.Configuration;

public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public override DateOnly Parse(object value)
    {
        if (value is DateOnly date)
        {
            return date;
        }

        return DateOnly.Parse(
            value.ToString()!,
            CultureInfo.InvariantCulture);
    }
}
