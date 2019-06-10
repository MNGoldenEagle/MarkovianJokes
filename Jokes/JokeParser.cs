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
    /// 
    /// It is unclear if the Markov algorithm should separate symbols into their own tokens or not.  Wikipedia doesn't seem to indicate one way or another.  This
    /// will separate them into their own tokens for now (just due to the small dataset), but eventually we may want to include the symbols in with the tokens.
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

            Cache.AddFirstWord(tokens[0]);
            tokens.Skip(1).ToList().ForEach(token => Cache.AddWord(token));

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                var token1 = tokens[i];
                var token2 = tokens[i + 1];

                Cache.AddWordPair(token1, token2);
            }
        }

        /// <summary>
        /// An enumeration indicating what token we are currently parsing.
        /// </summary>
        private enum Mode
        {
            /// <summary>
            /// A word token.
            /// </summary>
            WORD,
            /// <summary>
            /// A whitespace pseudo-token. Whitespace is omitted from the final token list.
            /// </summary>
            WHITESPACE,
            /// <summary>
            /// A symbol token.
            /// </summary>
            SYMBOLS
        }

        /// <summary>
        /// Splits the joke into word and symbol tokens and returns the resulting list of tokens.
        /// </summary>
        /// <param name="joke">The joke string</param>
        /// <returns></returns>
        private List<string> SplitIntoTokens(string joke)
        {
            var tokens = new List<string>();
            var currentMode = GetCharacterMode(joke[0]);
            var currentToken = new List<char>();

            for (int i = 0; i < joke.Length; i++)
            {
                var c = joke[i];
                var mode = GetCharacterMode(c);

                if (currentMode == mode)
                {
                    currentToken.Add(c);
                }
                else
                {
                    // Character mode has changed, which means current token is complete.
                    ProcessToken(tokens, currentMode, currentToken);

                    currentMode = mode;
                    currentToken = new List<char> { c };
                }
            }
            // Process final token and add the End-Of-Joke terminator.
            ProcessToken(tokens, currentMode, currentToken);
            tokens.Add("<eoj>");

            return tokens;
        }

        /// <summary>
        /// Processes the current token and adds the resulting token(s) to the list of tokens.
        /// </summary>
        /// <param name="tokens">The list of tokens for this joke</param>
        /// <param name="currentMode">The current token mode</param>
        /// <param name="currentToken">The current token</param>
        private void ProcessToken(List<string> tokens, Mode currentMode, List<char> currentToken)
        {
            switch (currentMode)
            {
                case Mode.WORD:
                    {
                        var preparedToken = PrepareWordToken(currentToken);
                        tokens.Add(preparedToken);
                        break;
                    }
                case Mode.WHITESPACE:
                    {
                        break;
                    }
                case Mode.SYMBOLS:
                    {
                        var preparedToken = PrepareSymbolToken(currentToken);
                        tokens.AddRange(preparedToken);
                        break;
                    }
            }
        }

        /// <summary>
        /// Prepares the word token. Word tokens are always lowercase, unless it is the word "I" or a variant thereof.
        /// </summary>
        /// <param name="characters">The characters of the current token</param>
        /// <returns>The resulting token</returns>
        private string PrepareWordToken(List<char> characters)
        {
            var word = new string(characters.ToArray());

            if (word.Equals("I") || word.StartsWith("I'") || word.StartsWith("I’"))
            {
                return word;
            }
            else
            {
                return word.ToLower();
            }
        }

        /// <summary>
        /// Prepares the symbol token. Symbols are always separated into their own tokens when adjacent to each other, unless
        /// it's a string of periods.
        /// </summary>
        /// <param name="characters">The characters of the current token</param>
        /// <returns>The resulting token(s)</returns>
        private List<string> PrepareSymbolToken(List<char> characters)
        {
            var result = new List<string>();

            if (characters.Count > 1 && characters[0] == '.' && characters[1] == '.')
            {
                var token = new string(characters.ToArray());
                result.Add(token);
                return result;
            }

            return characters.Select(c => c.ToString()).ToList();
        }

        /// <summary>
        /// Returns the character mode based on the character provided.
        /// </summary>
        /// <param name="c">The character</param>
        /// <returns>The determined mode</returns>
        private Mode GetCharacterMode(char c)
        {
            // Word characters are letters, digits, and apostrophes.
            if (Char.IsLetterOrDigit(c) || c == '\'' || c == '’')
            {
                return Mode.WORD;
            }
            // Let Unicode determine whitespace characters.
            if (Char.IsWhiteSpace(c))
            {
                return Mode.WHITESPACE;
            }
            // Let Unicode determine punctuation characters.
            if (Char.IsPunctuation(c))
            {
                return Mode.SYMBOLS;
            }
            // If somehow we reach this point, assume it's whitespace (in other words, ignore the character).
            return Mode.WHITESPACE;
        }
    }
}
