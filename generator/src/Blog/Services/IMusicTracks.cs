using System.Collections.Generic;

namespace Blog.Services
{
    public interface IMusicTracks
    {
        IReadOnlyCollection<MusicTrack> GetMusicTracks();
    }
}