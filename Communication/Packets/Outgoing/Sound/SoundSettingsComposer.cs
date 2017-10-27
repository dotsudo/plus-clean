namespace Plus.Communication.Packets.Outgoing.Sound
{
    using System.Collections.Generic;

    internal class SoundSettingsComposer : ServerPacket
    {
        public SoundSettingsComposer(ICollection<int> clientVolumes, bool chatPreference, bool invitesStatus, bool focusPreference, int friendBarState)
            : base(ServerPacketHeader.SoundSettingsMessageComposer)
        {
            foreach (var volumeValue in clientVolumes)
            {
                WriteInteger(volumeValue);
            }

            WriteBoolean(chatPreference);
            WriteBoolean(invitesStatus);
            WriteBoolean(focusPreference);
            WriteInteger(friendBarState);
            WriteInteger(0);
            WriteInteger(0);
        }
    }
}