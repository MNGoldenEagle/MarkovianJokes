using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Markov_Jokes.JokeSet.Model
{
    /// <summary>
    /// A page of jokes as returned by the icanhazdadjoke.com website.
    /// 
    /// All page structures are immutable.
    /// </summary>
    public struct Page
    {
        [JsonProperty(PropertyName = "current_page")]
        public uint CurrentPage { get; }

        [JsonProperty(PropertyName = "limit")]
        public int Limit { get; }

        [JsonProperty(PropertyName = "next_page")]
        public int NextPage { get; }

        [JsonProperty(PropertyName = "previous_page")]
        public int PreviousPage { get; }

        [JsonProperty(PropertyName = "status")]
        public HttpStatusCode Status { get; }

        [JsonProperty(PropertyName = "total_jokes")]
        public uint TotalJokes { get; }

        [JsonProperty(PropertyName = "total_pages")]
        public uint TotalPages { get; }

        [JsonProperty(PropertyName = "results")]
        public List<Joke> Jokes { get; }

        [JsonConstructor]
        public Page(uint currentPage, int limit, int nextPage, int prevPage, HttpStatusCode status, uint totalJokes, uint totalPages, List<Joke> jokes)
        {
            CurrentPage = currentPage;
            Limit = limit;
            NextPage = nextPage;
            PreviousPage = prevPage;
            Status = status;
            TotalJokes = totalJokes;
            TotalPages = totalPages;
            Jokes = jokes;
        }
    }
}
