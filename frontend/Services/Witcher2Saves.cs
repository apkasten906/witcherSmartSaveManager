using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using WitcherGuiApp.Models;

namespace WitcherGuiApp.Services
{
    public class Witcher2Saves : ClientAPI
    {        
        public Witcher2Saves(HttpClient http) : base(ConfigurationManager.AppSettings["ApiBaseUrl"], http) { }

        public async Task<IEnumerable<SaveFile>> GetWitcher2SavesAsync()
        {
            try
            {                
                return await GetAsync<IEnumerable<SaveFile>>($"saves/witcher2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ApiClient: {ex.Message}");                
                return new List<SaveFile>(); // Return an empty list to handle the error gracefully  
            }        
        }

    }
        
}
