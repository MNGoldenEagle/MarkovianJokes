using Markov_Jokes.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Markov_Jokes.Jokes
{
    /// <summary>
    /// Parses new jokes into tokens and persists them into the local cache.  Note this class uses parallel tasks, so the backing cache must be appropriately
    /// threadsafe to avoid data corruption.
    /// </summary>
    public class JokeParser
    {
        private readonly ICacheManager Cache;

        /// <summary>
        /// The main constructor for the Joke Parser.
        /// </summary>
        /// <param name="cache">The backing cache manager for jokes, tokens, and the token edge weights.</param>
        public JokeParser(ICacheManager cache)
        {
            Cache = cache;
        }

        /// <summary>
        /// Parses all of the jokes provided and stores them in the backing cache.
        /// </summary>
        /// <param name="jokes">The jokes to be parsed and added</param>
        /// <returns></returns>
        public async Task AddJokes(IEnumerable<string> jokes)
        {
            Cache.AddJokes(jokes);

            var tasks = new List<Task>();

            foreach (string joke in jokes)
            {
                tasks.Add(Task.Factory.StartNew(() => ProcessJoke(joke)));
            }

            await Task.WhenAll(tasks.ToArray());
            Cache.PersistNewEntries();
        }

        /// <summary>
        /// Processes the joke into tokens and pairs and sends to the cache.
        /// </summary>
        /// <param name="joke">The joke to parse.</param>
        private void ProcessJoke(string joke)
        {
            var tokens = SplitIntoTokens(joke);

            tokens.ToList().ForEach(token => Cache.AddWord(token));

            Cache.AddWordTriplet(TokenConstants.NO_LEADING, TokenConstants.NO_LEADING, tokens[0]);
            Cache.AddWordTriplet(TokenConstants.NO_LEADING, tokens[0], tokens[1]);

            for (int i = 0; i < tokens.Count - 2; i++)
            {
                var token1 = tokens[i];
                var token2 = tokens[i + 1];
                var token3 = tokens[i + 2];

                Cache.AddWordTriplet(token1, token2, token3);
            }
        }

        /// <summary>
        /// Splits the joke into tokens and returns the resulting list.
        /// </summary>
        /// <param name="joke">The joke string</param>
        /// <returns></returns>
        private List<string> SplitIntoTokens(string joke)
        {
            var tokens = joke.Split(null);
            var processedTokens = new List<string>();

            foreach (var token in tokens)
            {
                var processedToken = PrepareWordToken(token);
                processedTokens.Add(processedToken);
            }
            
            // Add the End-Of-Joke terminator.
            processedTokens.Add(TokenConstants.END_OF_JOKE);

            return processedTokens;
        }

        /// <summary>
        /// Prepares the word token. Word tokens are always lowercase, unless it is the word "I" or a variant thereof.
        /// </summary>
        /// <param name="word">The characters of the current token</param>
        /// <returns>The resulting token</returns>
        private string PrepareWordToken(string word)
        {
            if (word.Equals("I") || word.StartsWith("I'") || word.StartsWith("I’"))
            {
                return word;
            }
            else
            {
                return word.ToLower();
            }
        }
    }
}
