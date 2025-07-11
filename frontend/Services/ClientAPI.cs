using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// base class found here: https://stackoverflow.com/questions/65383186/using-httpclient-getfromjsonasync-how-to-handle-httprequestexception-based-on
namespace WitcherGuiApp.Services
{
    public abstract class ClientAPI(string baseRoute, HttpClient http)
    {
        protected readonly HttpClient Http = http;

        protected async Task<TReturn> GetAsync<TReturn>(string relativeUri)
        {
            HttpResponseMessage res = await Http.GetAsync($"{baseRoute}/{relativeUri}");
            if (res.IsSuccessStatusCode)
            {
                if (res.StatusCode == HttpStatusCode.NoContent)
                    return default(TReturn);
                else if (res.IsSuccessStatusCode)
                    return await res.Content.ReadFromJsonAsync<TReturn>();
                else
                    return default(TReturn);
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }

        protected async Task<TReturn> PostAsync<TReturn, TRequest>(string relativeUri, TRequest request)
        {
            HttpResponseMessage res = await Http.PostAsJsonAsync<TRequest>($"{baseRoute}/{relativeUri}", request);
            if (res.IsSuccessStatusCode)
            {
                if (res.StatusCode == HttpStatusCode.NoContent)
                    return default(TReturn);
                else if (res.IsSuccessStatusCode)
                    return await res.Content.ReadFromJsonAsync<TReturn>();
                else
                    return default(TReturn);
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }
    }
}