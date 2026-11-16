// Ian Freitas de Sousa
using AcademiaDoZe.Application.Enums;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class RestricoesFlagsDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppMatriculaRestricoes flags)
            return string.Empty;

        if (flags == AppMatriculaRestricoes.None)
            return AppMatriculaRestricoes.None.GetDisplayName();

        var partes = new List<string>();

        foreach (var item in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (item == AppMatriculaRestricoes.None)
                continue;

            if (flags.HasFlag(item))
                partes.Add(item.GetDisplayName());
        }

        return partes.Count > 0
            ? string.Join(", ", partes)
            : AppMatriculaRestricoes.None.GetDisplayName();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
