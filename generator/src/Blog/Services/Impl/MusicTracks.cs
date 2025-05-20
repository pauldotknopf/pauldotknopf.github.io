using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Blog.Services.Impl
{
    public class MusicTracks : IMusicTracks
    {
        private readonly IReadOnlyCollection<MusicTrack> _musicTracks;

        public MusicTracks(List<MusicTrack> musicTracks)
        {
            _musicTracks = new ReadOnlyCollection<MusicTrack>(musicTracks);
        }
        
        public IReadOnlyCollection<MusicTrack> GetMusicTracks()
        {
            return _musicTracks;
        }
    }
}