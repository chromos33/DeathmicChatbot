using System;
using BobDeathmic.Services.Helper;

namespace BobDeathmic.Services
{
    internal class SchedulerTaskWrapper
    {
        public NCrontab.CrontabSchedule Schedule { get; set; }
        public IScheduledTask Task { get; set; }
        public DateTime NextRunTime { get; set; }
        private bool Init = false;
        public bool ShouldRun(DateTime time)
        {
            if(Init)
            {
                Init = false;
                Increment();
                return false;
            }
            return NextRunTime.Hour == Schedule.GetNextOccurrence(time).Hour && NextRunTime.Minute == Schedule.GetNextOccurrence(time).Minute;
        }
        public void Increment()
        {
            NextRunTime = Schedule.GetNextOccurrence(DateTime.Now);
        }
        public void Initialize()
        {
            Init = true;
            NextRunTime = Schedule.GetNextOccurrence(DateTime.Now);
        }
    }
}