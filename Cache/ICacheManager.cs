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
        /// Adds the first word of a joke to the cache.
        /// </summary>
        /// <param name="word">The first word of a joke</param>
        void AddFirstWord(string word);

        /// <summary>
        /// Adds the jokes (pre-parsed) to the cache.
        /// </summary>
        /// <param name="jokes">The jokes to store in the cache</param>
        void AddJokes(IEnumerable<string> jokes);

        /// <summary>
        /// Adds the provided word to the cache.
        /// 
        /// Words added via this method are assumed not to be the first word of a joke.  If this is not the case, use the
        /// <see cref="AddFirstWord(string)"/> method instead.
        /// </summary>
        /// <param name="word">The word to add to the cache</param>
        void AddWord(string word);

        /// <summary>
        /// Adds the pair of words to the cache.  The cache will internally keep track of the number of times it encounters
        /// the pair of words.
        /// </summary>
        /// <param name="firstWord">The first word encountered</param>
        /// <param name="secondWord">The second word encountered</param>
        void AddWordPair(string firstWord, string secondWord);

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
        /// Gets a dictionary of valid starting words from the cache, where the key is the first word and the value is the number of occurrences.
        /// </summary>
        /// <returns>A dictionary of starting words</returns>
        IDictionary<string, int> GetStartingWords();

        /// <summary>
        /// Returns a dictionary of words that may follow the provided word, where the key is words that can follow and the value is the number of occurrences.
        /// </summary>
        /// <param name="word">The word</param>
        /// <returns>The potential following words and their weights, as a dictionary</returns>
        IDictionary<string, int> GetWeightsForWord(string word);
    }
}
