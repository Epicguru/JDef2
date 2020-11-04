using JDef.DummyTypes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JDef
{
    [CanBeDummy]
    public abstract class Def
    {
        [XmlIgnore]
        public string DefName { get; internal set; }

        [XmlIgnore]
        internal bool hasDummyTypes;

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

        /// <summary>
        /// Should be called by custom resolvers to indicate that this def has 1 or more field
        /// that is a dummy type and needs replacing.
        /// </summary>
        public void FlagDummyTypes()
        {
            this.hasDummyTypes = true;
        }

        public override string ToString()
        {
            return DefName;
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
