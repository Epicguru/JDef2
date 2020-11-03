using System.Collections.Generic;
using JDef;

namespace Testbed
{
    public class PersonDef : Def
    {
        public string Name;
        public int Age;

        public Def FavoritePerson;
        public List<Def> Friends;
        public Def[] Enemies;
        public Dictionary<string, Def> Others;
        public Dictionary<string, int> Test;
        public Shitbox Shitbox;

        public override string ToString()
        {
            return $"{Name}, age {Age}";
        }
    }

    public class Shitbox
    {
        public int Value;
        public Def Owner;
    }
}
