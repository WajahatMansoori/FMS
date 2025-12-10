using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers
{
    public class ConvertUtcToPakistanTimeHelper
    {
        public  DateTime ConvertUtcToPakistanTime(DateTime utcDateTime)
        {
            TimeZoneInfo pakistanTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, pakistanTimeZone);
        }

    }
}
