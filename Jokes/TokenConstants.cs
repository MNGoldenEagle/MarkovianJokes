using System;
using System.Collections.Generic;
using System.Text;

namespace Markov_Jokes.Jokes
{
    /// <summary>
    /// A class containing commonly shared tokens.
    /// </summary>
    public sealed class TokenConstants
    {
        /// <summary>
        /// Ensure this class cannot be instantiated.
        /// </summary>
        private TokenConstants()
        {

        }

        /// <summary>
        /// Indicates that there is no leading token.
        /// </summary>
        public const string NO_LEADING = "<not>";

        /// <summary>
        /// Indicates that the joke has terminated.
        /// </summary>
        public const string END_OF_JOKE = "<eoj>";
    }
}
