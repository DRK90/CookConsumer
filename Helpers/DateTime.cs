namespace CookConsumer.Helpers;

public static class DateTimeHelper
{
   public static DateTime ConvertToEasternTime(DateTime utcDateTime)
{
    var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, easternZone);
    }

    public static int CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        return age;
    }
}
