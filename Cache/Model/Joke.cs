using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Markov_Jokes.Cache.Model
{
    /// <summary>
    /// Model class for a Joke as used by the Entity Framework.
    /// </summary>
    [Table("Jokes")]
    public class Joke
    {
        /// <summary>
        /// The primary key of the joke entity.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The contents of the joke.
        /// </summary>
        [Required]
        public string Contents { get; set; }

        public override string ToString()
        {
            return string.Format("Joke ID: {0} Contents: [{1}]", Id, Contents);
        }
    }
}
