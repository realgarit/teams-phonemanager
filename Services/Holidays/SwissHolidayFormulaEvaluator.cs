using System;

namespace teams_phonemanager.Services.Holidays
{
    public static class SwissHolidayFormulaEvaluator
    {
        public static DateTime NewYear(int year) => new DateTime(year, 1, 1);
        public static DateTime Berchtoldstag(int year) => new DateTime(year, 1, 2);
        public static DateTime GoodFriday(int year) => EasterSunday(year).AddDays(-2);
        public static DateTime EasterMonday(int year) => EasterSunday(year).AddDays(1);
        public static DateTime AscensionDay(int year) => EasterSunday(year).AddDays(39);
        public static DateTime WhitMonday(int year) => EasterSunday(year).AddDays(50);
        public static DateTime CorpusChristi(int year) => EasterSunday(year).AddDays(60);
        public static DateTime SwissNationalDay(int year) => new DateTime(year, 8, 1);
        public static DateTime AssumptionDay(int year) => new DateTime(year, 8, 15);
        public static DateTime AllSaintsDay(int year) => new DateTime(year, 11, 1);
        public static DateTime ImmaculateConception(int year) => new DateTime(year, 12, 8);
        public static DateTime Christmas(int year) => new DateTime(year, 12, 25);
        public static DateTime StStephensDay(int year) => new DateTime(year, 12, 26);
        public static DateTime LabourDay(int year) => new DateTime(year, 5, 1);
        public static DateTime Epiphany(int year) => new DateTime(year, 1, 6);
        public static DateTime StJosephsDay(int year) => new DateTime(year, 3, 19);
        public static DateTime StPeterAndPaul(int year) => new DateTime(year, 6, 29);
        
        // Geneva-specific holidays
        public static DateTime JeuneGenevois(int year)
        {
            // Thursday after the first Sunday in September
            var firstSunday = new DateTime(year, 9, 1);
            while (firstSunday.DayOfWeek != DayOfWeek.Sunday)
                firstSunday = firstSunday.AddDays(1);
            return firstSunday.AddDays(4); // Thursday
        }
        
        public static DateTime RestaurationRepublique(int year) => new DateTime(year, 12, 31);
        
        // Glarus-specific holiday
        public static DateTime NaefelserFahrt(int year)
        {
            // First Thursday in April
            var april1 = new DateTime(year, 4, 1);
            while (april1.DayOfWeek != DayOfWeek.Thursday)
                april1 = april1.AddDays(1);
            return april1;
        }

        // Obwalden-specific: Saint Nicholas of Flüe Day
        public static DateTime BruderklausenFest(int year) => new DateTime(year, 9, 25);

        // Appenzell Innerrhoden-specific (inner part only)
        public static DateTime MauritiusTag(int year) => new DateTime(year, 9, 22);

        // Vaud-specific: Monday after the third Sunday in September (Jeûne fédéral)
        public static DateTime LundiDuJeune(int year)
        {
            var sept1 = new DateTime(year, 9, 1);
            while (sept1.DayOfWeek != DayOfWeek.Sunday)
                sept1 = sept1.AddDays(1);
            var thirdSunday = sept1.AddDays(14);
            return thirdSunday.AddDays(1); // Monday
        }

        // Neuchâtel-specific: Instauration de la République
        public static DateTime RepublicDayNeuchatel(int year) => new DateTime(year, 3, 1);

        // Jura-specific: Commémoration du plébiscite jurassien
        public static DateTime JurassianIndependenceDay(int year) => new DateTime(year, 6, 23);

        // Computus - Meeus/Jones/Butcher algorithm for Gregorian Easter Sunday
        public static DateTime EasterSunday(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31; // 3=March, 4=April
            int day = ((h + l - 7 * m + 114) % 31) + 1;
            return new DateTime(year, month, day);
        }
    }
}


