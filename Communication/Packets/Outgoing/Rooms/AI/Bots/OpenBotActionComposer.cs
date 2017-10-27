namespace Plus.Communication.Packets.Outgoing.Rooms.AI.Bots
{
    using HabboHotel.Rooms;

    internal class OpenBotActionComposer : ServerPacket
    {
        public OpenBotActionComposer(RoomUser botUser, int actionId, string botSpeech)
            : base(ServerPacketHeader.OpenBotActionMessageComposer)
        {
            WriteInteger(botUser.BotData.Id);
            WriteInteger(actionId);
            if (actionId == 2)
            {
                WriteString(botSpeech);
            }
            else if (actionId == 5)
            {
                WriteString(botUser.BotData.Name);
            }
        }
    }
}