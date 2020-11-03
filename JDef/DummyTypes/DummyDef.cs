namespace JDef.DummyTypes
{
    internal class DummyDef : Def, IDummyType
    {
        private readonly DefDatabase db;

        public DummyDef(string name, DefDatabase db)
        {
            base.DefName = name;
            this.db = db;
        }

        public object GetRealObject()
        {
            return db.GetNamed(DefName);
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                DummyDef dDef => this.DefName == dDef.DefName,
                Def def => this.DefName == def.DefName,
                _ => false
            };
        }

        public override int GetHashCode()
        {
            if (base.DefName == null)
                return base.GetHashCode();

            return base.DefName.GetHashCode();
        }

        public override string ToString()
        {
            return $"<{DefName ?? "null-name"}>";
        }
    }
}
