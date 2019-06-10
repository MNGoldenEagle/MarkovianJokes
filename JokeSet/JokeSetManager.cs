using Markov_Jokes.JokeSet.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Markov_Jokes.JokeSet
{
    public class JokeSetManager
    {
        private static readonly Uri endpoint = new Uri("https://icanhazdadjoke.com/search");

        private const sbyte MAX_PER_REQUEST = 30;

        private readonly int TOTAL_JOKES;

        public volatile int JokesRead;

        public JokeSetManager(int totalJokes = 0, int totalJokesRead = 0)
        {
            if (totalJokes < 0)
            {
                TOTAL_JOKES = int.MaxValue;
            } else
            {
                TOTAL_JOKES = totalJokes;
            }
            JokesRead = totalJokesRead;
        }

        public async Task<List<string>> GetJokes()
        {
            var jokes = new List<string>();
            var page = JokesRead / MAX_PER_REQUEST + 1;
            var offset = JokesRead % MAX_PER_REQUEST;
            int totalRead = JokesRead;

            do
            {
                var result = await GetJokePage((uint)page);

                if (result.TotalJokes <= JokesRead)
                {
                    break;
                }

                var skipLast = Math.Max(0, (totalRead + result.Jokes.Count) - TOTAL_JOKES);
                var currentJokes = result.Jokes.Skip(offset).SkipLast(skipLast).Select(joke => joke.Contents);
                totalRead += currentJokes.Count();
                jokes.AddRange(currentJokes);
                offset = 0;
                if (page == result.NextPage)
                {
                    break;
                }
                page = result.NextPage;
            } while (page > 0 && totalRead < TOTAL_JOKES);

            JokesRead = totalRead;

            return jokes;
        }

        public async Task<Page> GetJokePage(uint page)
        {
            var requestUri = new Uri(endpoint, string.Format("?page={0}&limit={1}", page, MAX_PER_REQUEST));
            var request = WebRequest.Create(requestUri);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "Funny Joke Generator");

            using (var httpResponse = await request.GetResponseAsync())
            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var data = await reader.ReadToEndAsync();
                var response = JsonConvert.DeserializeObject<Page>(data);
                if (response.Status != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("Error occurred retrieving jokes: {0}, content is: {1}", response.Status, data));
                }
                return response;
            }
        }
    }
}
