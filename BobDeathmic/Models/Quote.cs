using System;
using System.ComponentModel.DataAnnotations;

namespace BobDeathmic.Models
{
    public class Quote
    {
        public int Id { get; set; }
        [Required] public string Streamer { get; set; }
        [Required] public DateTime Created { get; set; }
        [Required] public string Text { get; set; }

        public override string ToString()
        {
            return $"\"{Text}\" - {Streamer}, {Created:MMMM YYYY} (ID {Id})";
        }
    }
}