using System;
using BobDeathmic.Services.Helper;

namespace BobDeathmic.Services
{
    internal class SchedulerTaskWrapper
    {
        public NCrontab.CrontabSchedule Schedule { get; set; }
        public IScheduledTask Task { get; set; }
        public DateTime NextRunTime { get; set; }
        public bool Init;
        public SchedulerTaskWrapper()
        {
            NextRunTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
        }
        public bool ShouldRun()
        {
            if(Init)
            {
                Init = false;
                Increment();
                return false;
            }
            if(NextRunTime < Schedule.GetNextOccurrence(DateTime.Now))
            {
                return true;
            }
            return NextRunTime.Hour == Schedule.GetNextOccurrence(DateTime.Now).Hour && NextRunTime.Minute == Schedule.GetNextOccurrence(DateTime.Now).Minute;
        }
        public void Increment()
        {
            NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
        }
    }
}