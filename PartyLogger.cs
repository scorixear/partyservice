using System;
using System.Collections.Generic;

namespace PartyService
{
    public class PartyLogger
    {
        private Dictionary<int, string> players;

        public PartyLogger()
        {
            players = new Dictionary<int, string>();
        }

        public void AddPlayer(int id, string name)
        {
            if(!players.ContainsKey(id))
                players.Add(id, name);
        }

        public void RemovePlayer(int id)
        {
            if(players.ContainsKey(id))
            {
                OnPlayerLeftEvent(players[id]);
                Console.WriteLine("Player left Party: " + players[id]);
                players.Remove(id);
            }
        }

        public delegate void PlayerLeftEventHandler(string playerName);
        public event PlayerLeftEventHandler PlayerLeftEvent;
        private void OnPlayerLeftEvent(string playerName)
        {
            PlayerLeftEvent?.Invoke(playerName);
        }
    }
}
