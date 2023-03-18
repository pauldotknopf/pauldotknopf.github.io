using System;
using System.Collections.Generic;

namespace Blog.Models
{
    public class SessionReportModel
    {
        public string TimeZone { get; set; }

        public WindUnits WindUnits { get; set; }

        public TempUnits TempUnits { get; set; }

        public List<SessionReportSpotModel> SessionSpots { get; set; }
    }

    public class SessionReportSpotModel
    {
        public SessionReportSpotModel()
        {
            Data = new List<SessionReportSpotDataModel>();
        }

        public string Name { get; set; }

        public int ModelId { get; set; }

        public int SpotId { get; set; }

        public DateTimeOffset FromUtc { get; set; }

        public DateTimeOffset FromLocal { get; set; }

        public DateTimeOffset ToUtc { get; set; }

        public DateTimeOffset ToLocal { get; set; }

        public List<SessionReportSpotDataModel> Data { get; set; }
    }

    public class SessionReportSpotDataModel
    {
        public DateTimeOffset ModelTimeUtc { get; set; }

        public DateTimeOffset ModelTimeLocal { get; set; }

        public decimal WindSpeed { get; set; }

        public int WindDirection { get; set; }

        public string WindDirectionText { get; set; }

        public decimal Temp { get; set; }
    }

    public enum WindUnits
    {
        Mph,
        Kph,
        Mps,
        Knots
    }

    public enum TempUnits
    {
        Fahrenheit,
        Celsius
    }
}

