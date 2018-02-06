using System;

namespace LegendGenerator.App.Utils
{
    public class FeedbackEventArgs : EventArgs
    {
        public FeedbackEventArgs(bool close, bool resetForm)
        {
            this.ShouldClose = close;
            this.ResetForm = resetForm;
        }
        public bool ShouldClose { get; set; }
        public bool ResetForm { get; set; }
        //public string Reason { get; set; }
    }
}