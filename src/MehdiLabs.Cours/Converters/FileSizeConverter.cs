using System.Globalization;
using System.Windows.Data;

namespace MehdiLabs.Cours.Converters;

public class FileSizeConverter : IValueConverter
{
    private static readonly string[] Suffixes = { "B", "KB", "MB", "GB", "TB" };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            if (bytes == 0) return "0 B";

            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            
            // Éviter l'out of bounds
            place = Math.Min(place, Suffixes.Length - 1);
            
            return $"{num} {Suffixes[place]}";
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
