using System.Globalization;

namespace ShortUrl.Common.Utility.ToolsHelpers
{
    public static class DateTools
    {

        #region Private Variables

        private static PersianCalendar persianCalendar;
        private static string[] _persianMonthNames = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        private static List<string> _persianDayNames = new List<string>() { "یکشنبه", "دوشنبه", "سه شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
        private static List<string> _englishDayNames = new List<string>() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        #endregion

        public static PersianCalendar PersianCalendar => persianCalendar ??= new PersianCalendar();

        public static string GetPersianDate(DateTime? date)
        {
            if (date == null || date.Value == DateTime.MinValue)
                return string.Empty;

            var day = PersianCalendar.GetDayOfMonth(date.Value);
            var month = PersianCalendar.GetMonth(date.Value);

            return PersianCalendar.GetYear(date.Value) + "/"
                + (month < 10 ? "0" + month : month.ToString()) + "/"
                + (day < 10 ? "0" + day : day.ToString());
        }

        /// <summary>
        /// تاریخ و زمان را برمی گرداند
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetPersianDateTime(DateTime? date)
        {
            return GetPersianDateTime(date, false);
        }

        /// <summary>
        /// تاریخ و زمان را برمی گرداند
        /// </summary>
        /// <param name="date"></param>
        /// <param name="toLocalTime">آیا زمان را به زمان محلی برگرداند</param>
        /// <returns></returns>
        public static string GetPersianDateTime(DateTime? date, bool toLocalTime = false)
        {
            if (date == null)
                return string.Empty;
            if (toLocalTime)
                date = date.Value.ToLocalTime();
            return GetPersianDate(date) + " " + date.Value.ToString("HH:mm:ss");
        }

        public static string GetPersianDate(string dateString) => DateTime.TryParse(dateString, out var date) ? GetPersianDate(date) : string.Empty;
        public static string GetPersianMonthName() => GetPersianMonthName(DateTime.Now);
        public static string GetPersianMonthName(DateTime dt) => GetPersianMonthName(PersianCalendar.GetMonth(dt));
        public static string GetPersianMonthName(int Month) => _persianMonthNames[Month - 1];
        public static string GetPersianDateByMonthName() => GetPersianDateByMonthName(DateTime.Now);
        public static string GetPersianDateByMonthName(DateTime dt) => PersianCalendar.GetDayOfMonth(dt) + " " + _persianMonthNames[PersianCalendar.GetMonth(dt) - 1] + " " + PersianCalendar.GetYear(dt);
        public static string GetPersianDateByMonthName2(DateTime dt) => PersianCalendar.GetDayOfMonth(dt) + " " + _persianMonthNames[PersianCalendar.GetMonth(dt) - 1];
        public static string GetPersianDayName(DateTime dt) => _persianDayNames[(int)dt.DayOfWeek];
        public static string GetPersianDayName(DayOfWeek dayOfWork) => _persianDayNames[(int)dayOfWork];
        public static int GetPersianDayOfMonth(DateTime dt) => PersianCalendar.GetDayOfMonth(dt);

        public static DateTime? PersianToDateTime(string persianDate)
        {
            try
            {
                // 09/09/1395
                if (string.IsNullOrWhiteSpace(persianDate))
                    return null;

                var arr = persianDate.Replace("-", "/").Split('/');
                if (arr.Length != 3)
                    return null;

                int year, month, day;
                month = int.Parse(arr[1]);

                if (arr[2].Length == 4)
                {
                    year = int.Parse(arr[2]);
                    day = int.Parse(arr[0]);
                }
                else if (arr[0].Length == 4)
                {
                    year = int.Parse(arr[0]);
                    day = int.Parse(arr[2]);
                }
                else
                {
                    int first = int.Parse(arr[0]);
                    int last = int.Parse(arr[2]);

                    if (first <= 31 && last > 31)
                    {
                        day = first;
                        if (last < 100)
                            year = last + 1300;
                        else
                            year = last;
                    }
                    else if (last <= 31 && first > 31)
                    {
                        day = last;
                        if (first < 100)
                            year = first + 1300;
                        else
                            year = first;
                    }
                    else
                    {
                        year = day = 0;
                    }
                }

                return PersianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                throw new Exception("PersianDateTime is not correct format! " + persianDate, ex);
            }
        }
        public static DateTime? PersianToDateTime(int Year, int Month, int Day, int Hour, int Minute, int Second, int MilliSecond)
            => PersianCalendar.ToDateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond);

    }
}
