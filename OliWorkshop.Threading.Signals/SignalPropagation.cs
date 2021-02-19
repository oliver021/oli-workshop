namespace OliWorkshop.Threading.Signals
{
    public readonly struct SignalPropagation
    {
        public SignalPropagation(string name, SignalHandler handler)
        {
            Name = name;
            Handler = handler;
        }

        public string Name { get;  }

        public SignalHandler Handler { get; }
    }
}