using System;
using System.Collections.Generic;

namespace Blog.Models
{
    public class SessionReportModel
    {
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

        public string TimeZoneName { get; set; }

        public string TimeZoneNameShort { get; set; }

        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }

        public List<SessionReportSpotDataModel> Data { get; set; }
    }

    public class SessionReportSpotDataModel
    {
        public DateTimeOffset ModelTime { get; set; }

        public decimal WindSpeed { get; set; }

        public int WindDirection { get; set; }

        public string WindDirectionText { get; set; }

        public decimal Temp { get; set; }
    }
}

