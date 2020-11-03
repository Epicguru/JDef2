using JDef;
using System;

namespace Testbed
{
    class Program
    {
        static void Main(string[] _)
        {
            Console.WriteLine("Hello World!");

            using var db = new DefDatabase();

            const string toLoadFrom = "./Content/";
            db.LoadFromDir(toLoadFrom);
            db.Process();

            Console.WriteLine("\n");

            Console.WriteLine($"Finished loading, got {db.DefCount}");
            Console.WriteLine();

            foreach (var person in db.GetAllOfType<PersonDef>())
            {
                Console.WriteLine($"[{person.DefName}] {person}");
                if(person.FavoritePerson != null)
                    Console.WriteLine($"{person.Name}'s favorite person is {person.FavoritePerson}");
                if(person.Friends != null)
                    foreach (var friend in person.Friends)
                        Console.WriteLine($"Friend: {friend}");
                if (person.Enemies != null)
                    foreach (var enemy in person.Enemies)
                        Console.WriteLine($"Enemy: {enemy}");
                if(person.Others != null)
                    foreach (var pair in person.Others)
                        Console.WriteLine($"{pair.Key}: {pair.Value}");
                if (person.Test != null)
                    foreach (var pair in person.Test)
                        Console.WriteLine($"{pair.Key}: {pair.Value}");

                if (person.Shitbox != null)
                {
                    Console.WriteLine("Shitbox:");
                    Console.WriteLine(" -Value: " + person.Shitbox.Value);
                    Console.WriteLine(" -Owner: " + person.Shitbox.Owner);
                }

                if (person is MageDef mage)
                {
                    if(mage.Minions != null)
                        foreach(var minion in mage.Minions)
                            Console.WriteLine($"Minion [{minion.GetType().Name}] {minion.Name} with {minion.LegCount} legs.");
                }
                Console.WriteLine("\n");
            }

            Console.ReadLine();
        }
    }
}
