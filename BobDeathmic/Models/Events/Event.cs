using BobDeathmic.Models.Events.ManyMany;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.Events
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public List<EventDateTemplate> EventDateTemplates { get; set; }
        public List<ChatUserModel_Event> Members { get; set; }

        public string AdministratorID { get; set; }
        public ChatUserModel Admin { get; set; }

        public List<EventDate> EventDates { get; set; }


        public Event()
        {
            EventDateTemplates = new List<EventDateTemplate>();
            Members = new List<ChatUserModel_Event>();
            EventDates = new List<EventDate>();
        }

        public List<ChatUserModel> getMembers()
        {
            List<ChatUserModel> Users = new List<ChatUserModel>();
            foreach(ChatUserModel_Event member in Members)
            {
                Users.Add(member.ChatUserModel);
            }
            return Users;
        }
        public List<AppointmentRequest> GenerateAppointmentRequests(EventDate eventDate)
        {
            List<AppointmentRequest> tmp = new List<AppointmentRequest>();
            foreach(ChatUserModel_Event MemberRelation in Members)
            {
                tmp.Add(new AppointmentRequest { Owner = MemberRelation.ChatUserModel, EventDate = eventDate });
            }
            return tmp;
        }
    }
}
