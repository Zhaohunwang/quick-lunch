using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class DateTimeToRelativeConverter : IValueConverter
    {
        public static readonly DateTimeToRelativeConverter Instance = new DateTimeToRelativeConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not DateTime dateTime) return string.Empty;
            
            // 如果是默认时间，显示"从未打开"
            if (dateTime == default || dateTime == DateTime.MinValue)
            {
                return "从未打开";
            }

            var now = DateTime.UtcNow;
            var diff = now - dateTime.ToUniversalTime();

            return diff.TotalMinutes switch
            {
                < 1 => "刚刚",
                < 60 => $"{diff.TotalMinutes:0}分钟前",
                < 1440 => $"{diff.TotalHours:0}小时前",
                < 2880 => "昨天",
                < 4320 => "前天",
                < 10080 => $"{diff.TotalDays:0}天前",
                < 43200 => $"{diff.TotalDays / 7:0}周前",
                < 525600 => $"{diff.TotalDays / 30:0}个月前",
                _ => $"{diff.TotalDays / 365:0}年前"
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
