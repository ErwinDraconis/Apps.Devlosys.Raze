namespace System
{
    public static class DateTimeExtensions
    {
        public static bool IsSameOrAfter(this DateTime date1, DateTime date2)
        {
            return DateTime.Compare(date1, date2) >= 0;
        }

        public static bool IsSameDay(this DateTime date1, DateTime date2)
        {
            return date1.ToString("ddMMyyyy") == date2.ToString("ddMMyyyy");
        }

        public static bool IsSame(this DateTime date1, DateTime date2)
        {
            return date1.ToString("ddMMyyyyHHmm") == date2.ToString("ddMMyyyyHHmm");
        }
    }
}
