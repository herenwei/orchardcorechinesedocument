using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace OrchardCore.Modules
{
    public class LocalClock : ILocalClock
    {
        private readonly IEnumerable<ITimeZoneSelector> _timeZoneSelectors;
        private readonly IClock _clock;
        private ITimeZone _timeZone;

        public LocalClock(IEnumerable<ITimeZoneSelector> timeZoneSelectors, IClock clock)
        {
            _timeZoneSelectors = timeZoneSelectors;
            _clock = clock;
        }

        public Task<DateTimeOffset> LocalNowAsync
        {
            get
            {
                return GetLocalNowAsync();
            }
        }

        private async Task<DateTimeOffset> GetLocalNowAsync()
        {
            return _clock.ConvertToTimeZone(_clock.UtcNow, await GetLocalTimeZoneAsync());
        }

        public async Task<ITimeZone> GetLocalTimeZoneAsync()
        {
            // Caching the result per request
            if (_timeZone == null)
            {
                _timeZone = await LoadLocalTimeZoneAsync();
            }

            return _timeZone;
        }

        public async Task<DateTimeOffset> ConvertToLocalAsync(DateTimeOffset dateTimeOffSet)
        {
            var localTimeZone = await GetLocalTimeZoneAsync();
            var dateTimeZone = ((TimeZone)localTimeZone).DateTimeZone;
            var offsetDateTime = OffsetDateTime.FromDateTimeOffset(dateTimeOffSet);
            return offsetDateTime.InZone(dateTimeZone).ToDateTimeOffset();
        }

        public async Task<DateTime> ConvertToUtcAsync(DateTime dateTime)
        {
            var localTimeZone = await GetLocalTimeZoneAsync();
            var dateTimeZone = ((TimeZone)localTimeZone).DateTimeZone;
            var localDate = LocalDateTime.FromDateTime(dateTime);
            return dateTimeZone.AtStrictly(localDate).ToDateTimeUtc();
        }

        private async Task<ITimeZone> LoadLocalTimeZoneAsync()
        {
            var timeZoneResults = new List<TimeZoneSelectorResult>();

            foreach (var timeZoneSelector in _timeZoneSelectors)
            {
                var timeZoneResult = await timeZoneSelector.GetTimeZoneAsync();

                if (timeZoneResult != null)
                {
                    timeZoneResults.Add(timeZoneResult);
                }
            }

            if (timeZoneResults.Count == 0)
            {
                return _clock.GetSystemTimeZone();
            }
            else if (timeZoneResults.Count > 1)
            {
                timeZoneResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));
            }

            foreach(var result in timeZoneResults)
            {
                var value = await result.TimeZoneId();

                if (!String.IsNullOrEmpty(value))
                {
                    return _clock.GetTimeZone(value);
                }
            }

            return _clock.GetSystemTimeZone();
        }
    }
}
