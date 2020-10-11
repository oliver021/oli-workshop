using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OliWorkshop.Threading.Reactive
{
    /// <summary>
    /// The chanell pipe item to manage the input
    /// </summary>
    public interface IChannelPipe
    {
        string NameIdentifier { get; }

        Stream PipeStream(Stream message);
    }
}
