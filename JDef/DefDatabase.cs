using JDef.DummyTypes;
using JXml;
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
        public int DefCount
        {
            get
            {
                return allDefs.Count;
            }
        }

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

        public IEnumerable<T> GetAllOfType<T>()
        {
            foreach (var def in allDefs)
            {
                if (def is T t)
                    yield return t;
            }
        }

        public void AddRootTypeResolver(IRootTypeSerializer rts)
        {
            this.loader?.AddRootTypeResolver(rts);
        }

        public void AddCustomResolver(Type type, Func<CustomResolverArgs, object> func)
        {
            this.loader?.AddCustomResolver(type, func);
        }

        public void LoadFromDir(string dir)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot load, dir not found: {dir}");

            string[] files = Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories);
            string[] data = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                data[i] = File.ReadAllText(path);
            }
            loader.Load(data);
        }

        public void Load(string xmlData)
        {
            if (string.IsNullOrWhiteSpace(xmlData))
                throw new ArgumentNullException(nameof(xmlData));

            loader.Load(xmlData);
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
                if (def.hasDummyTypes)
                {
                    DummyTypeReplacer.ReplaceDummyTypes(def);
                }
                def.PostProcess();
            }
        }

        private void AddDef(Def def)
        {
            if (def == null)
                return;

            namedDefs.Add(def.DefName, def);
            allDefs.Add(def);

            Console.WriteLine($"Loaded a {def.GetType().FullName}: {def}");
        }

        private void RemoveDef(Def def)
        {
            if (def == null)
                return;

            namedDefs.Add(def.DefName, def);
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
