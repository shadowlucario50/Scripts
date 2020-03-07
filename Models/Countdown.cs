
using System;

namespace Script.Models
{
    public class Countdown
    {
        public string Text { get; set; }
        public DateTime TargetTime { get; set; }

        public Countdown(string text, DateTime targetTime) 
        {
            this.Text = text;
            this.TargetTime = targetTime;
        }
    }
}