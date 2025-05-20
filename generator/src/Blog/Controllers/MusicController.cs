using System.Linq;
using Blog.Misc;
using Blog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    public class MusicController : Controller
    {
        private readonly IMusicTracks _musicTracks;

        public MusicController(IMusicTracks musicTracks)
        {
            _musicTracks = musicTracks;
        }

        public ActionResult Index()
        {
            return View(_musicTracks.GetMusicTracks().ToList());
        }
        
        public ActionResult Track([FromRouteData]MusicTrack track)
        {
            return View(track);
        }

        public ActionResult Artist(string artistName)
        {
            return View(_musicTracks.GetMusicTracks().Where(x => x.Artist == artistName).ToList());
        }

        public ActionResult Tuning(string tuning)
        {
            return View(_musicTracks.GetMusicTracks().Where(x => x.Guitar != null && x.Guitar.Tuning == tuning).ToList());
        }

        public ActionResult Guitar()
        {
            return View(_musicTracks.GetMusicTracks().Where(x => x.Guitar != null).ToList());
        }

        public ActionResult Drums()
        {
            return View(_musicTracks.GetMusicTracks().Where(x => x.Drum != null).ToList());
        }
    }
}