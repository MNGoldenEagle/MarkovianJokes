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

        private ConcurrentDictionary<string, int> Tokens = new ConcurrentDictionary<string, int>();

        private ConcurrentDictionary<string, ConcurrentDictionary<string, int>> Occurrences = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

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

        public void AddFirstWord(string word)
        {
            AddWord(word, true);
        }

        public void AddWord(string word)
        {
            AddWord(word, false);
        }

        private void AddWord(string word, bool first = false)
        {
            int amount = first ? 1 : 0;
            Tokens.AddOrUpdate(word, amount, (key, value) => value + amount);
        }

        public void AddWordPair(string firstWord, string secondWord)
        {
            var newDict = new ConcurrentDictionary<string, int>();
            newDict[secondWord] = 1;
            Occurrences.AddOrUpdate(firstWord, newDict, (key, value) =>
            {
                value.AddOrUpdate(secondWord, 1, (key2, value2) => value2 + 1);
                return value;
            });
        }

        // TODO: This method could use further parallelization.
        public void PersistNewEntries()
        {
            // Determine which words are new and which words already exist.  For words that already exist, update the existing entities.  Otherwise, add them to the
            // database context.  Then persist the changes made.
            var persistableWords = Tokens.Select(entry => new Word { Content = entry.Key, StartCount = entry.Value });
            var existingWords = Database.Words.Where(word => Tokens.Keys.Contains(word.Content)).ToList();
            var newWords = new List<Word>();
            foreach (var word in persistableWords)
            {
                var existing = existingWords.FirstOrDefault(entry => WORD_COMPARE.Equals(word, entry));
                if (existing != null)
                {
                    existing.StartCount += word.StartCount;
                }
                else
                {
                    newWords.Add(word);
                }
            };
            Database.Words.AddRange(newWords);
            Database.SaveChanges();

            // Determine which weights are new and which ones already exist.  The methodology here is the same as we did for words, but because weights have a more complex structure,
            // it takes additional work to convert them into the appropriate data structure. This could use reworking to not use dictionaries.
            var persistableWeights = new List<Weight>();
            foreach (var entry in Occurrences)
            {
                var firstWord = Database.Words.Single(word => entry.Key.Equals(word.Content));
                foreach (var counts in entry.Value)
                {
                    var secondWord = Database.Words.Single(word => counts.Key.Equals(word.Content));

                    var weight = new Weight { Word = firstWord, WordId = firstWord.Id, FollowingWord = secondWord, FollowingWordId = secondWord.Id, Occurrences = counts.Value };
                    persistableWeights.Add(weight);
                }
            }

            var existingWeights = new List<Weight>();
            foreach (var weight in persistableWeights)
            {
                var existing = Database.WordWeights.SingleOrDefault(persisted => weight.WordId == persisted.WordId && weight.FollowingWordId == persisted.FollowingWordId);
                if (existing == null)
                {
                    continue;
                }
                existingWeights.Add(existing);
                existing.Occurrences += weight.Occurrences;
            }
            persistableWeights = persistableWeights.Except(existingWeights, WEIGHT_COMPARE).ToList();
            Database.WordWeights.AddRange(persistableWeights);
            Database.SaveChanges();

            Tokens = new ConcurrentDictionary<string, int>();
            Occurrences = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
        }

        public IDictionary<string, int> GetStartingWords()
        {
            return Database.Words.Where(word => word.StartCount > 0).ToDictionary(word => word.Content, word => word.StartCount);
        }

        public IDictionary<string, int> GetWeightsForWord(string word)
        {
            return Database.WordWeights.Include(weight => weight.Word).Include(weight => weight.FollowingWord).Where(weight => weight.Word.Content.Equals(word))
                .ToDictionary(weight => weight.FollowingWord.Content, weight => weight.Occurrences);
        }

        public int GetTotalJokes()
        {
            return Database.Jokes.Count();
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
            model.Entity<Weight>().HasKey(weight => new { weight.WordId, weight.FollowingWordId });
        }
    }
}
