using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Markov_Jokes.Cache.Model
{
    /// <summary>
    /// Model class for words (a.k.a. tokens) as used by the Entity Framework.
    /// </summary>
    [Table("Words")]
    public class Word
    {
        /// <summary>
        /// The primary key of the Word entity.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The content of the word.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// The number of occurrences this word has begun a joke.
        /// </summary>
        [Required]
        public int StartCount { get; set; }

        public override string ToString()
        {
            return string.Format("Word ID: {0} Content: [{1}] Start occurrences: {2}", Id, Content, StartCount);
        }
    }

    /// <summary>
    /// A comparer class that only evaluates whether the contents of the words are identical (ignores any differences in ID or start counts).
    /// </summary>
    public class WordComparer : IEqualityComparer<Word>
    {
        public bool Equals(Word x, Word y)
        {
            return x.Content.Equals(y.Content);
        }

        public int GetHashCode(Word obj)
        {
            return 37 ^ obj.Content.GetHashCode();
        }
    }
}
