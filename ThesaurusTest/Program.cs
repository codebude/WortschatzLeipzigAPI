using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WortschatzLeipzigAPI;

namespace ThesaurusTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter one word: ");
            
            var word = Console.ReadLine();
            Console.Clear();

            var synonyms = Thesaurus.GetSynonyms(word);
            synonyms.ForEach(synonym => Console.WriteLine(synonym));
            Console.WriteLine("Found " + synonyms.Count + " synonyms.");

            Console.ReadKey();
        }
    }
}
