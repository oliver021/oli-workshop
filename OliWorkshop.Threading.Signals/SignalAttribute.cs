using System;
using System.Collections.Generic;
using System.Text;

namespace OliWorkshop.Threading.Signals
{
    public class SignalAttribute : Attribute
    {
        public SignalAttribute(string signalName)
        {
            SignalName = signalName ?? throw new ArgumentNullException(nameof(signalName));
        }

        public string SignalName { get; }
    }
}
