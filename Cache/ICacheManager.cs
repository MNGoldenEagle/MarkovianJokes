using System;
using System.Collections.Generic;
using System.Text;

namespace Markov_Jokes.Cache
{
    /// <summary>
    /// An interface for the cache manager of jokes (pre-parsed and post-parsed).
    /// </summary>
    public interface ICacheManager : IDisposable
    {
        /// <summary>
        /// Adds the jokes (pre-parsed) to the cache.
        /// </summary>
        /// <param name="jokes">The jokes to store in the cache</param>
        void AddJokes(IEnumerable<string> jokes);

        /// <summary>
        /// Adds the provided word to the cache.
        /// </summary>
        /// <param name="word">The word to add to the cache</param>
        void AddWord(string word);

        /// <summary>
        /// Adds the triplet of words to the cache.  The cache will internally keep track of the number of times it encounters
        /// the triplet.
        /// </summary>
        /// <param name="firstWord">The first word encountered</param>
        /// <param name="secondWord">The second word encountered</param>
        /// <param name="followingWord">The word that follows the first two</param>
        void AddWordTriplet(string firstWord, string secondWord, string thirdWord);

        /// <summary>
        /// Returns the number of jokes that have been processed.
        /// </summary>
        /// <returns>The total number of jokes</returns>
        int GetTotalJokes();

        /// <summary>
        /// Persists all new entries to the persisted cache (if persisted).  This method may throw exceptions if any issues occur while persisting the entries.
        /// </summary>
        void PersistNewEntries();

        /// <summary>
        /// Returns a dictionary of words that may follow the provided words, where the key is words that can follow and the value is the number of occurrences.
        /// </summary>
        /// <param name="word1">The first word</param>
        /// <param name="word2">The second word</param>
        /// <returns>The potential following words and their weights, as a dictionary</returns>
        IDictionary<string, int> GetWeightsForWord(string word1, string word2);
    }
}
