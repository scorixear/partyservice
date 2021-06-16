using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PartyService
{
    public class PacketHandler
    {
        private PartyLogger partyService;
        private HttpClient client;
        //private const string itemsMappingUrl = "https://kellerus.de/lootlogger/items.json";
        //private const string eventsMappingUrl = "https://kellerus.de/lootlogger/events.json";
        private bool isInitialized = false;
        private Dictionary<string, int> eventDictionary = new Dictionary<string, int>();
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public PacketHandler(PartyLogger partyService)
        {
            this.partyService = partyService;
            client = new HttpClient();
        }
        
        public async void OnEvent(byte code, Dictionary<byte, object> parameters)
        {
            if (!this.isInitialized)
            {
                await this.InitializeAsync();
            }
            if (code == 2)
            {
                return;
            }
            object val;
            parameters.TryGetValue(252, out val);
            if (val == null)
            {
                return;
            }
            int iCode = 0;
            if (!int.TryParse(val.ToString(), out iCode))
            {
                return;
            }

            Console.WriteLine(iCode);
            if (eventDictionary.ContainsKey("evPartyPlayerJoined") && iCode == eventDictionary["evPartyPlayerJoined"])
            {
                this.OnPlayerPartyJoin(parameters);
            }
            else if(eventDictionary.ContainsKey("evPartyPlayerLeft") && iCode == eventDictionary["evPartyPlayerLeft"])
            {
                this.OnPlayerPartyLeft(parameters);
            }

        }

        private void OnPlayerPartyJoin(Dictionary<byte, object> parameters)
        {
            try
            {
                string leader = ((string[])parameters[5])[0];
                string invitation = ((string[])parameters[5])[1];
                int playerId = int.Parse(parameters[0].ToString());
                partyService.AddPlayer(playerId, invitation);
            } catch { }
        }

        private void OnPlayerPartyLeft(Dictionary<byte, object> parameters)
        {
            
                int leave = int.Parse(parameters[1].ToString());
                partyService.RemovePlayer(leave);
            
        }

        

       
        public class Event
        {
            public string Name { get; set; }
            public int Code { get; set; }
        }

        

        private async Task InitializeAsync()
        {
            semaphore.Wait();
            try
            {
                if(eventDictionary.Count == 0)
                {
                    var response = await this.client.GetAsync(new Uri(Config.instance.EventsUrl));
                    var content = await response.Content.ReadAsStringAsync();
                    List<Event> eventList = JsonConvert.DeserializeObject<List<Event>>(content);
                    eventList.ForEach(entry => eventDictionary.Add(entry.Name, entry.Code));
                    this.isInitialized = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
