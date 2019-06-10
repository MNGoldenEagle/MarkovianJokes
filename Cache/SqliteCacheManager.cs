using Markov_Jokes.Cache.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Markov_Jokes.Cache
{
    /// <summary>
    /// A cache manager that utilizes Sqlite to cache the data locally.  This implementation is only appropriate for single clients; if multiple processes/clients
    /// require access, it is recommended to use an alternative implementation, such as RedisCacheManager.
    /// </summary>
    public class SqliteCacheManager : ICacheManager
    {
        private readonly CacheContext Database = new CacheContext();

        private bool alreadyDisposed = false;

        private ConcurrentDictionary<string, bool> Tokens = new ConcurrentDictionary<string, bool>();

        private ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, int>>> Occurrences =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, int>>>();

        private readonly WordComparer WORD_COMPARE = new WordComparer();

        private readonly WeightComparer WEIGHT_COMPARE = new WeightComparer();

        /// <summary>
        /// Creates a new cache manaager. Each instance will own its own connection to the database.
        /// </summary>
        public SqliteCacheManager()
        {
            
        }

        public void AddJokes(IEnumerable<string> jokes)
        {
            var persistedJokes = jokes.Select(joke => new Joke { Contents = joke });

            Database.Jokes.AddRange(persistedJokes);
            Database.SaveChanges();
        }

        
        public void AddWord(string word)
        {
            Tokens.AddOrUpdate(word, true, (key, value) => true);
        }

        public void AddWordTriplet(string word1, string word2, string followingWord)
        {
            // In case one of the words does not exist in the dictionary, we will insert a pre-built dictionary with the proper values initialized.
            var newDict = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
            var newNestedDict = new ConcurrentDictionary<string, int>();
            newNestedDict[followingWord] = 1;
            newDict[word2] = newNestedDict;

            Occurrences.AddOrUpdate(word1, newDict, (key, value) =>
            {
                value.AddOrUpdate(word2, newNestedDict, (key2, value2) => {
                    value2.AddOrUpdate(followingWord, 1, (key3, value3) => value3 + 1);
                    return value2;
                });
                return value;
            });
        }

        // TODO: This method could use further parallelization.
        public void PersistNewEntries()
        {
            // To make this parallelize properly, grab all the existing words and their corresponding IDs.
            var existingWordIds = Database.Words.ToDictionary(word => word.Content);
            // Do the same with the existing weights.
            var existingWeights = GetExistingWeights();
            
            // Determine which words are new and which words already exist.  For words that don't exist, add them to the database context.  Then persist the changes made.
            var persistableWords = Tokens.Select(entry => new Word { Content = entry.Key });
            var newWords = new ConcurrentQueue<Word>();
            Parallel.ForEach(persistableWords, word =>
            {
                if (!existingWordIds.Keys.Contains(word.Content))
                {
                    newWords.Enqueue(word);
                }
            });
            Database.Words.AddRange(newWords);
            Database.SaveChanges();

            if (newWords.Count > 0)
            {
                existingWordIds = Database.Words.ToDictionary(word => word.Content);
            }

            // Determine which weights are new and which ones already exist.  The methodology here is the same as we did for words, but because weights have a more complex structure,
            // it takes additional work to convert them into the appropriate data structure. This could use reworking to not use dictionaries.
            var persistableWeights = ConvertDictionariesToWeights(existingWordIds, existingWeights);

            var newWeights = new ConcurrentQueue<Weight>();
            Parallel.ForEach(persistableWeights, weight =>
            {
                var existing = existingWeights.GetValueOrDefault(weight.Word1Id)?.GetValueOrDefault(weight.Word2Id)?.GetValueOrDefault(weight.FollowingWordId);
                if (existing == null)
                {
                    newWeights.Enqueue(weight);
                }
                else
                {
                    existing.Occurrences += weight.Occurrences;
                }
            });
            Database.WordWeights.AddRange(newWeights);
            Database.SaveChanges();

            Tokens = new ConcurrentDictionary<string, bool>();
            Occurrences = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, int>>>();
        }

        private ConcurrentQueue<Weight> ConvertDictionariesToWeights(Dictionary<string, Word> existingWords, Dictionary<long, Dictionary<long, Dictionary<long, Weight>>> existingWeights)
        {
            var result = new ConcurrentQueue<Weight>();
            
            // We'll parallelize at the first word as that's going to have the most possible entries.  Parallelizing at each level would potentially overwhelm the thread pool,
            // so doing so at only the top level should be sufficient.
            Parallel.ForEach(Occurrences, level1 =>
            {
                var firstWord = existingWords[level1.Key];

                foreach (var level2 in level1.Value)
                {
                    var secondWord = existingWords[level2.Key];

                    foreach (var level3 in level2.Value)
                    {
                        var followWord = existingWords[level3.Key];

                        var weight = new Weight
                        {
                            Word1 = firstWord,
                            Word1Id = firstWord.Id,
                            Word2 = secondWord,
                            Word2Id = secondWord.Id,
                            FollowingWord = followWord,
                            FollowingWordId = followWord.Id,
                            Occurrences = level3.Value
                        };

                        result.Enqueue(weight);
                    }
                }
            });
            return result;
        }

        public IDictionary<string, int> GetWeightsForWord(string word1, string word2)
        {
            return Database.WordWeights.Include(weight => weight.Word1).Include(weight => weight.Word2).Include(weight => weight.FollowingWord)
                .Where(weight => weight.Word1.Content.Equals(word1) && weight.Word2.Content.Equals(word2))
                .ToDictionary(weight => weight.FollowingWord.Content, weight => weight.Occurrences);
        }

        public int GetTotalJokes()
        {
            return Database.Jokes.Count();
        }

        private Dictionary<long, Dictionary<long, Dictionary<long, Weight>>> GetExistingWeights()
        {
            var weights = Database.WordWeights.GroupBy(weight => weight.Word1Id)
                .ToDictionary(group1 => group1.Key,
                              group1 => group1.GroupBy(entry => entry.Word2Id).ToDictionary(group2 => group2.Key,
                                                                                            group2 => group2.ToDictionary(weight => weight.FollowingWordId, weight => weight)));
            return weights;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!alreadyDisposed && disposing)
            {
                Database.Dispose();
                alreadyDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    /// <summary>
    /// A helper class for managing the database context. This class owns the actual database connection and the properties that reflect the tables.
    /// </summary>
    internal class CacheContext : DbContext
    {
        internal DbSet<Joke> Jokes { get; set; }

        internal DbSet<Weight> WordWeights { get; set; }

        internal DbSet<Word> Words { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=cache.db");
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            // The word table has a unique index on the actual word.
            model.Entity<Word>().HasIndex(word => word.Content).IsUnique();
            // The weight table has a composite key for the words that correspond to the weight.
            model.Entity<Weight>().HasKey(weight => new { weight.Word1Id, weight.Word2Id, weight.FollowingWordId });
        }
    }
}
