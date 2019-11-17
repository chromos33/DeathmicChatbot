using BobDeathmic.ViewModels.ReactDataClasses.EventDateFinder.OverView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic;

namespace BobDeathmic.ViewModels.ReactDataClasses.EventDateFinder.OverView
{
    public class OverView
    {
        public List<Calendar> Calendars { get; set; }
        public string AddCalendarLink { get; set; }
    }
}
