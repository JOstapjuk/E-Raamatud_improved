using System.Globalization;

namespace E_Raamatud
{
    /// <summary>
    /// Extracts just the filename from a full path for display in the audio file list.
    /// Binding: Text="{Binding ., Converter={x:Static local:FileNameConverter.Instance}}"
    /// </summary>
    public class FileNameConverter : IValueConverter
    {
        public static readonly FileNameConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string path ? System.IO.Path.GetFileName(path) : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Inverts a bool — used to disable the "Lisa raamat" button while uploading.
    /// Binding: IsEnabled="{Binding IsUploading, Converter={x:Static local:InverseBoolConverter.Instance}}"
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public static readonly InverseBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}
