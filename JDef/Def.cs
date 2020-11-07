using JDef.DummyTypes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JDef
{
    [CanBeDummy]
    public abstract class Def
    {
        public static event Action<Def, string, Exception> OnValidationError;
        public static event Action<Def, string> OnValidationWarning;

        /// <summary>
        /// Changes a def's name. You will normally never want to do this.
        /// Only use if you know exactly what you are doing.
        /// </summary>
        /// <param name="def">The def.</param>
        /// <param name="newName">The new name. Must not be null or blank.</param>
        public static void SetName(Def def, string newName)
        {
            if (def == null)
                throw new ArgumentNullException(nameof(def));
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentNullException(nameof(newName), "The new name must not be null or empty.");

            def.DefName = newName;
        }

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
        /// Called after the def has been loaded, and after post processing.
        /// Should be used to report error in the def.
        /// Default implementation does nothing.
        /// </summary>
        public virtual void Validate()
        {

        }

        /// <summary>
        /// Should be called by custom resolvers to indicate that this def has 1 or more field
        /// that is a dummy type and needs replacing.
        /// </summary>
        public void FlagDummyTypes()
        {
            this.hasDummyTypes = true;
        }

        /// <summary>
        /// Logs a validation error.
        /// </summary>
        public void ValidateError(string error, Exception e = null)
        {
            OnValidationError?.Invoke(this, error, e);
        }

        /// <summary>
        /// Logs a validation warning.
        /// </summary>
        public void ValidateWarn(string warning)
        {
            OnValidationWarning?.Invoke(this, warning);
        }

        public override string ToString()
        {
            return DefName;
        }
    }
}
