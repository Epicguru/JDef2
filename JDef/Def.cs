using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JDef
{
    public abstract class Def
    {
        [XmlIgnore]
        public string Name { get; internal set; }

        public virtual void PostProcess()
        {
            if (postProcessActions == null)
                return;

            foreach(var pp in postProcessActions)
            {
                pp?.Invoke(this);
            }

            postProcessActions.Clear();
            postProcessActions = null;
        }

        internal List<Action<Def>> postProcessActions;

        internal void AddPostProcessAction(Action<Def> action)
        {
            if (postProcessActions == null)
                postProcessActions = new List<Action<Def>>();

            postProcessActions.Add(action);
        }

        public override string ToString()
        {
            return $"[{GetType().Name}] {Name}";
        }

        internal static void Error(string msg, Exception e = null)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"JDef Error: {msg}");
            if(e != null)
                Console.WriteLine($"Exception:\n{e}");
            Console.ForegroundColor = oldColor;
        }
    }
}
