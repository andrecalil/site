using MiniBiggy;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFloripa.Data
{
    public class GitHubDataStore : IDataStore
    {
        private string URL;

        public GitHubDataStore(string url)
        {
            URL = url;
        }

        public Task<byte[]> ReadAllAsync()
        {
            string result = string.Empty;

            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("https://raw.githubusercontent.com/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = httpClient.GetAsync(URL).Result;

                if (response.IsSuccessStatusCode)
                    result = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            { throw e; }

            return Task.FromResult(Encoding.UTF8.GetBytes(result));
        }

        public Task WriteAllAsync(byte[] list)
        {
            throw new NotImplementedException();
        }
    }
}