namespace Blog
{
    public class MusicTrack
    {
        public string Artist { get; set; }
        
        public string Song { get; set; }
        
        public GuitarModel Guitar { get; set; }
        
        public DrumModel Drum { get; set; }
        
        public class BaseInstrumentModel
        {
            public string TabUrl { get; set; }
            
            public string BackingTrackUrl { get; set; }
        }
        
        public class GuitarModel : BaseInstrumentModel
        {
            public string Tuning { get; set; }
        }

        public class DrumModel : BaseInstrumentModel
        {
            
        }
    }
}