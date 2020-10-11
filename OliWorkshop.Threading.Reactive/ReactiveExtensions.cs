using System;
using System.Collections.Generic;
using System.Text;

namespace OliWorkshop.Threading.Reactive
{
    /// <summary>
    /// The set of extensions to add many features to reactive artifacts
    /// </summary>
    public static class ReactiveExtensions
    {
        public static void PostString(this BroadcastConcurrent broadcast,string channel, string message, Encoding encoding = null)
        {
            if (broadcast is null)
            {
                throw new ArgumentNullException(nameof(broadcast));
            }

            if (encoding is null)
            {
                encoding = Encoding.UTF8;
            }

            broadcast.PostMessage(channel, encoding.GetBytes(message));
        }
    }
}
