using Newtonsoft.Json;

namespace Markov_Jokes.JokeSet.Model
{
    /// <summary>
    /// A model representation of the Joke datatype as provided by the icanhazdadjoke website.
    /// 
    /// All joke structures are immutable.
    /// </summary>
    public struct Joke
    {
        /// <summary>
        /// The ID of the joke.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; }

        /// <summary>
        /// The contents of the joke.
        /// </summary>
        [JsonProperty(PropertyName = "joke")]
        public string Contents { get; }

        /// <summary>
        /// The model 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contents"></param>
        [JsonConstructor]
        public Joke(string id, string contents)
        {
            ID = id;
            Contents = contents;
        }
    }
}