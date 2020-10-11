using System;
using System.IO;

namespace OliWorkshop.Threading.Reactive
{
    /// <summary>
    /// The structure that represent the message
    /// </summary>
    public struct BroadcastMessage
    {
        /// <summary>
        /// Initialize message with stream
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="messageStream"></param>
        public BroadcastMessage(string channel, Stream messageStream) : this()
        {
            Channel = channel;
            MessageStream = messageStream;
        }

        /// <summary>
        /// Initialize message with bytes
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="messageStream"></param>
        public BroadcastMessage(string channel, byte[] messagBytes) : this()
        {
            Channel = channel;
            MessagBytes = messagBytes;
        }

        public string Channel { get; }
        Stream MessageStream { get; }
        byte[] MessagBytes { get; }

        /// <summary>
        /// Get content as array bytes
        /// </summary>
        /// <returns></returns>
        public byte[] GetContentInBytes()
        {
            if (MessagBytes is null)
            {
                if (MessageStream.Length < 1)
                {
                    return Array.Empty<byte>();
                }
                else
                {
                    byte[] content = new byte[MessageStream.Length];
                    MessageStream.Read(content, 0, (int)MessageStream.Length);
                    return content;
                }
            }

            return MessagBytes;
        }

        /// <summary>
        /// Get content as stream
        /// </summary>
        /// <returns></returns>
        public Stream GetContentAsStream()
        {
            if (MessageStream is null)
            {
                return new MemoryStream(MessagBytes);
            }

            return MessageStream;
        }
    }
}