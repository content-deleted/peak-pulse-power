using GameStateManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final {
    public class SongSelect : MenuScreen {
        public struct songInfo {
            public string songPath;
            public string songName;
        }

        class SongEntry : MenuEntry {
            public songInfo songInfo;
            public SongEntry(songInfo s) : base(s.songName) {
                songInfo = s;
            }
        }


            songInfo[] songs;

        public SongSelect() : base("Song Select") {
            
            songs = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Songs").Where(f => f.Substring(f.LastIndexOf('.')).Equals(".mp3")).Select(fullPath => {
                var temp = fullPath.LastIndexOf("\\Songs") + 7;
                var length = fullPath.LastIndexOf(".") - temp;
                return new songInfo {
                    songName = fullPath.Substring(temp, length),
                    songPath = fullPath
                };
                }).ToArray();
            
            foreach (songInfo song in songs) {
                SongEntry temp = new SongEntry(song);
                temp.Selected += SelectedSong;// attach an event here
                MenuEntries.Add(temp);
            }

        }

        void SelectedSong(object sender, PlayerIndexEventArgs e) {
            songInfo song = (sender as SongEntry).songInfo;
             ScreenManager.AddScreen(new Gameplay(song, e.PlayerIndex), null);
        }

    
    }
}