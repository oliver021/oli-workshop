using System;
using System.Collections.Generic;
using System.Text;

namespace OliWorkshop.Threading.Signals
{
    public class SlotAttribute : Attribute
    {
        public SlotAttribute(string signalRecept)
        {
            SignalRecept = signalRecept ?? throw new ArgumentNullException(nameof(signalRecept));
        }

        public string SignalRecept { get; }
    }
}
