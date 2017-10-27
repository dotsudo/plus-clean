namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Moodlight
{
    using HabboHotel.Items.Data.Moodlight;

    internal class MoodlightConfigComposer : ServerPacket
    {
        public MoodlightConfigComposer(MoodlightData moodlightData)
            : base(ServerPacketHeader.MoodlightConfigMessageComposer)
        {
            WriteInteger(moodlightData.Presets.Count);
            WriteInteger(moodlightData.CurrentPreset);

            var i = 1;
            foreach (var preset in moodlightData.Presets)
            {
                WriteInteger(i);
                WriteInteger(preset.BackgroundOnly ? 2 : 1);
                WriteString(preset.ColorCode);
                WriteInteger(preset.ColorIntensity);
                i++;
            }
        }
    }
}