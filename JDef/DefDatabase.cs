using System;
using System.Collections.Generic;
using System.IO;

namespace JDef
{
    /// <summary>
    /// A definition database stores all loaded definitions.
    /// Definitions loaded into the database can automatically reference each other.
    /// </summary>
    public class DefDatabase : IDisposable
    {
        private List<Def> allDefs = new List<Def>();
        private Dictionary<string, Def> namedDefs = new Dictionary<string, Def>();
        private DefLoader loader;

        public DefDatabase()
        {
            loader = new DefLoader(this);
        }

        public Def GetNamed(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (namedDefs.TryGetValue(name, out var def))
                return def;
            return null;
        }

        public T GetNamed<T>(string name) where T : Def
        {
            return GetNamed(name) as T;
        }

        public void LoadFromDir(string dir)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot load, dir not found: {dir}");

            string[] files = Directory.GetFiles(dir, "*.xml");
            string[] data = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                data[i] = File.ReadAllText(path);
            }
            loader.Load(data);
        }

        public void Process()
        {
            var defs = loader.Process();
            foreach(var def in defs)
            {
                AddDef(def);
            }
            PostProcess();
        }

        private void PostProcess()
        {
            foreach(var def in allDefs)
            {
                def.PostProcess();
            }
        }

        private void AddDef(Def def)
        {
            if (def == null)
                return;

            namedDefs.Add(def.Name, def);
            allDefs.Add(def);

            Console.WriteLine($"Loaded a {def.GetType().FullName}: {def}");
        }

        private void RemoveDef(Def def)
        {
            if (def == null)
                return;

            namedDefs.Add(def.Name, def);
            allDefs.Add(def);
        }

        public void Dispose()
        {
            loader?.Dispose();
            loader = null;
            allDefs?.Clear();
            allDefs = null;
            namedDefs?.Clear();
            namedDefs = null;
        }
    }
}
