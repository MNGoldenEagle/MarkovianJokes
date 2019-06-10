using System;
using Markov_Jokes.Cache;
using Markov_Jokes.Jokes;
using Markov_Jokes.JokeSet;
using Nito.AsyncEx;

namespace Markov_Jokes
{
    public class FunnyJoke
    {
        public static void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
        }

        public static async void MainAsync(string[] args)
        {
            ushort numberOfJokes = 0;
            if (args.Length == 0 || !ushort.TryParse(args[0], out numberOfJokes))
            {
                Console.WriteLine("First argument must be a positive number indicating the number of jokes to create.");
                return;
            }

            using (var cache = new SqliteCacheManager())
            {
                Console.WriteLine("Retrieving funnnnnnny jokes!");
                var jokeManager = new JokeSetManager(-1, cache.GetTotalJokes());
                var jokes = await jokeManager.GetJokes();
                var parser = new JokeParser(cache);
                await parser.AddJokes(jokes);

                Console.WriteLine("Creating funnnnnnnnny jokes!");
                var generator = new JokeGenerator(cache);
                for (var i = 0; i < numberOfJokes; i++)
                {
                    Console.WriteLine(generator.GenerateJoke());
                }
            }

            Console.WriteLine();
        }
    }
}
