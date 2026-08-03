using System.Data;
using System.Globalization;
using Dapper;

namespace Puentes.Infrastructure.Configuration;

public sealed class DateTimeOffsetTypeHandler
    : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override void SetValue(
        IDbDataParameter parameter,
        DateTimeOffset value)
    {
        parameter.Value = value.ToString("O", CultureInfo.InvariantCulture);
    }

    public override DateTimeOffset Parse(object value)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset;
        }

        return DateTimeOffset.Parse(
            value.ToString()!,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
    }
}
