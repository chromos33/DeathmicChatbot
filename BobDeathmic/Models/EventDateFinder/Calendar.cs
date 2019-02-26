using BobDeathmic.Models.EventDateFinder.ManyMany;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.EventDateFinder
{
    public class Calendar
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public List<EventDateTemplate> EventDateTemplates { get; set; }
        public List<ChatUserModel_Calendar> Members { get; set; }

        public string AdministratorID { get; set; }
        public ChatUserModel Admin { get; set; }

        public List<EventDate> EventDates { get; set; }


        public Calendar()
        {
            EventDateTemplates = new List<EventDateTemplate>();
            Members = new List<ChatUserModel_Calendar>();
            EventDates = new List<EventDate>();
        }

        public bool isMember(ChatUserModel user)
        {
            if(Members.Where(x => x.ChatUserModelID == user.Id).Count() > 0)
            {
                return true;
            }
            return false;
        }
        public List<ChatUserModel> getMembers()
        {
            List<ChatUserModel> Users = new List<ChatUserModel>();
            foreach(ChatUserModel_Calendar member in Members)
            {
                Users.Add(member.ChatUserModel);
            }
            return Users;
        }
        public List<AppointmentRequest> GenerateAppointmentRequests(EventDate eventDate)
        {
            List<AppointmentRequest> tmp = new List<AppointmentRequest>();
            foreach(ChatUserModel_Calendar MemberRelation in Members)
            {
                tmp.Add(new AppointmentRequest { Owner = MemberRelation.ChatUserModel, EventDate = eventDate });
            }
            return tmp;
        }
    }
}
