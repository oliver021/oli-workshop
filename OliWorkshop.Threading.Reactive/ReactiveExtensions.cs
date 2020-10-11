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
        /// <summary>
        /// Post content message as string with char encode type
        /// Note: for default is utf8
        /// </summary>
        /// <param name="broadcast"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="encoding"></param>
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

        /// <summary>
        /// Get string with a char encoding type
        /// Note: for default is utf8
        /// </summary>
        /// <param name="message"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetString(this BroadcastMessage message, Encoding encoding = null)
        {
            if (encoding is null)
            {
                encoding = Encoding.UTF8;
            }

            return encoding.GetString(message.GetContentInBytes());
        }
    }
}
