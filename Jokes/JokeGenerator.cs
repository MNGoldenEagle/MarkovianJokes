using Markov_Jokes.Cache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Markov_Jokes.Jokes
{
    /// <summary>
    /// Generates jokes using words from the provided cache.  The "jokes" are generated using the Markov chain algorithm.
    /// </summary>
    public class JokeGenerator
    {
        private static readonly Random RANDOM = new Random();

        private static readonly TextInfo TEXT_HELPER = CultureInfo.InvariantCulture.TextInfo;

        private readonly ICacheManager Cache;

        /// <summary>
        /// Creates a new JokeGenerator instance with the provided cache.
        /// </summary>
        /// <param name="cache">The backing cache</param>
        public JokeGenerator(ICacheManager cache)
        {
            Cache = cache;
        }

        /// <summary>
        /// Generates a very hilarious joke that will be very funny upon reading.
        /// </summary>
        /// <returns>the generated string</returns>
        public string GenerateJoke()
        {
            var builder = new StringBuilder();
            bool firstWord = true;
            bool start = true;
            bool startQuote = false;
            var currentWord = GetWord(Cache.GetStartingWords());

            // Loop until we reach the end of our joke.
            while (!currentWord.Equals(TokenConstants.END_OF_JOKE))
            {
                var addedWord = currentWord;
                var wordIsTerminating = IsTerminating(currentWord);

                // If we're not at the beginning of the joke, prefix with a space.
                if (!start)
                {
                    addedWord = " " + addedWord;
                }

                // If this is the first word in a sentence, title-case it.
                if (firstWord)
                {
                    addedWord = TEXT_HELPER.ToTitleCase(addedWord);
                    firstWord = false;
                }

                // If the word is a terminator (something that ends the sentence), ensure that next sentence is capitalized.
                if (wordIsTerminating)
                {
                    firstWord = true;
                }

                // Make sure we keep our quotes balanced.
                if (currentWord.Count(c => c == '"' || c == '“' || c == '”') % 2 == 1)
                {
                    startQuote = !startQuote;
                }

                start = false;
                builder.Append(addedWord);

                currentWord = GetWord(Cache.GetWeightsForWord(currentWord));
            }

            // If quotes are unbalanced, add a quote to the end.  Who knows if it'll make sense?
            if (startQuote)
            {
                builder.Append('"');
            }

            return builder.ToString();
        }

        private bool IsTerminating(string word)
        {
            return word.IndexOfAny(new char[] { '.', '?', '!' }) >= 0;
        }

        /// <summary>
        /// Returns the next word based on the potential wordset provided.
        /// </summary>
        /// <param name="potentials">The potential candidates</param>
        /// <returns></returns>
        private string GetWord(IDictionary<string, int> potentials)
        {
            var wordSet = potentials.Where(entry => entry.Value != 0).ToDictionary(pair => pair.Key, pair => pair.Value);
            var words = wordSet.Keys;
            var weights = wordSet.Values;
            var totalWeight = weights.Sum();
            var selected = RANDOM.Next(0, totalWeight);

            foreach (var entry in wordSet)
            {
                selected -= entry.Value;

                if (selected < 0)
                {
                    return entry.Key;
                }
            }

            return wordSet.Last().Key;
        }
    }
}
