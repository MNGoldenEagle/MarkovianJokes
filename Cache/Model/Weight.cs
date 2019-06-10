using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Markov_Jokes.Cache.Model
{
    /// <summary>
    /// Model class for Markov weights as used by the Entity Framework.
    /// </summary>
    [Table("Weights")]
    public class Weight
    {
        /// <summary>
        /// The ID of the first word. This is required to form the composite key.
        /// </summary>
        [Required]
        public long Word1Id { get; set; }

        /// <summary>
        /// The ID of the second word. This is required to form the composite key.
        /// </summary>
        [Required]
        public long Word2Id { get; set; }

        /// <summary>
        /// The ID of the following word. This is required to form the composite key.
        /// </summary>
        [Required]
        public long FollowingWordId { get; set; }

        /// <summary>
        /// The first word. This is required for relational modeling such that Entity Framework can automatically manage it.
        /// </summary>
        [Required]
        public Word Word1 { get; set; }

        /// <summary>
        /// The second word. This is required for relational modeling such that Entity Framework can automatically manage it.
        /// </summary>
        [Required]
        public Word Word2 { get; set; }

        /// <summary>
        /// The following word. This is required for relational modeling such that Entity Framework can automatically manage it.
        /// </summary>
        [Required]
        public Word FollowingWord { get; set; }

        /// <summary>
        /// The number of occurrences this particular word combination has been encountered.
        /// </summary>
        [Required]
        public int Occurrences { get; set; }

        public override string ToString()
        {
            return string.Format("Chain Weight First Word: [{0}] Second Word: [{1}] Following Word: [{2}] Occurrences: {3}", Word1, Word2, FollowingWord, Occurrences);
        }
    }

    /// <summary>
    /// A comparer class that only evaluates whether the first and second words are identical (ignores any difference in occurrence counts).
    /// </summary>
    public class WeightComparer : IEqualityComparer<Weight>
    {
        public bool Equals(Weight x, Weight y)
        {
            return x.Word1Id.Equals(y.Word1Id) && x.Word2Id.Equals(y.Word2Id) && x.FollowingWordId.Equals(y.FollowingWordId);
        }

        public int GetHashCode(Weight obj)
        {
            return 51 ^ (int)obj.Word1Id ^ (int)(obj.Word2Id << 16) ^ (int)((long.MaxValue - obj.FollowingWordId) >> 32);
        }
    }
}
