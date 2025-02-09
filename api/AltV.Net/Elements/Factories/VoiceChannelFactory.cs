using System;
using AltV.Net.Elements.Entities;

namespace AltV.Net.Elements.Factories
{
    public class VoiceChannelFactory : IBaseObjectFactory<IVoiceChannel>
    {
        public IVoiceChannel Create(ICore core, IntPtr channelPointer)
        {
            return new VoiceChannel(core, channelPointer);
        }
    }
}