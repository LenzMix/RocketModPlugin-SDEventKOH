using Rocket.API;
using System.Collections.Generic;
using UnityEngine;

namespace SDEventKoH
{
    public class Config : IRocketPluginConfiguration, IDefaultable
    {
        public void LoadDefaults()
        {
            this.UI = 27701;
            this.MinPlayers = 20;
            this.MinTime = 1800;
            this.MaxTime = 3200;
            this.EventTime = 600;
            this.CaptureTime = 60;
            this.Locations = new List<Loc>();
            this.Rewards = new List<Reward>
            {
                new Reward
                {
                    Name = "Maple-Gang",
                    Rewards = new List<string>
                    {
                        "i %playerid% 363 1",
                        "i %playerid% 17 7"
                    }
                }
            };
        }

        public ushort UI;
        public int MinPlayers;
        public int MinTime;
        public int MaxTime;
        public int EventTime;
        public int CaptureTime;
        public List<Loc> Locations;
        public List<Reward> Rewards;
    }

    public class Reward
    {
        public string Name;
        public List<string> Rewards;
    }

    public class Loc
    {
        public string Name;
        public Vector3 pos;
        public float radius;
    }
}