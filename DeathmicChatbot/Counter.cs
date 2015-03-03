#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using DeathmicChatbot.Interfaces;

#endregion


namespace DeathmicChatbot
{
    public class Counter : ICounter
    {
        private readonly List<CountItem> _countItems = new List<CountItem>();

        #region ICounter Members

        public event EventHandler<CounterEventArgs> CountRequested;
        public event EventHandler<CounterEventArgs> ResetRequested;
        public event EventHandler<CounterEventArgs> StatRequested;

        public void Count(string sName)
        {
            var countItem =
                _countItems.FirstOrDefault(
                    item => item.Name == sName.ToLower().Trim());

            if (countItem == null)
            {
                countItem = new CountItem
                {
                    Name = sName.ToLower().Trim(),
                    Started = DateTime.Now
                };
                _countItems.Add(countItem);
            }

            countItem.Hits++;

            if (CountRequested == null)
                return;

            CountRequested(this,
                           new CounterEventArgs(
                               string.Format("Counter '{0}' is now at {1}.",
                                             sName,
                                             countItem.Hits)));
        }

        public void CounterReset(string sName)
        {
            var countItem =
                _countItems.FirstOrDefault(
                    item => item.Name == sName.ToLower().Trim());

            CounterStats(sName);

            if (countItem == null)
                return;

            _countItems.Remove(countItem);

            if (ResetRequested == null)
                return;

            ResetRequested(this,
                           new CounterEventArgs(
                               string.Format(
                                   "Counter '{0}' has now been reset.", sName)));
        }

        public void CounterStats(string sName)
        {
            if (StatRequested == null)
                return;

            var countItem =
                _countItems.FirstOrDefault(
                    item => item.Name == sName.ToLower().Trim());

            if (countItem == null)
            {
                StatRequested(this,
                              new CounterEventArgs(
                                  string.Format("Counter '{0}' not found.",
                                                sName)));
            }
            else
            {
                var timeSpan = DateTime.Now - countItem.Started;

                var timesPerHour = timeSpan.TotalHours < 1
                                       ? ""
                                       : string.Format(
                                           "{0} times per hour, or ",
                                           Math.Round(
                                               countItem.Hits /
                                               timeSpan.TotalHours,
                                               3));

                var sMessage =
                    string.Format(
                        "Counter '{0}' was started at {1} ({2} ago). It has since been called {3} times, that is {4}{5} times per minute, or once every {6} minutes.",
                        sName,
                        countItem.Started,
                        timeSpan.ToString(timeSpan.Days == 0
                                              ? "h':'mm':'ss"
                                              : "d' days 'h':'mm':'ss"),
                        countItem.Hits,
                        timesPerHour,
                        Math.Round(
                            countItem.Hits /
                            (timeSpan.TotalMinutes >= 1
                                 ? timeSpan.TotalMinutes
                                 : Math.Ceiling(timeSpan.TotalMinutes)),
                            3),
                        Math.Round(timeSpan.TotalMinutes / countItem.Hits, 3));

                StatRequested(this, new CounterEventArgs(sMessage));
            }
        }

        #endregion
    }
}