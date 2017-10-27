namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using System.Linq;
    using HabboHotel.Rooms;

    internal class GetRoomBannedUsersComposer : ServerPacket
    {
        public GetRoomBannedUsersComposer(Room instance)
            : base(ServerPacketHeader.GetRoomBannedUsersMessageComposer)
        {
            WriteInteger(instance.Id);

            WriteInteger(instance.GetBans().BannedUsers().Count); //Count
            foreach (var id in instance.GetBans().BannedUsers().ToList())
            {
                var data = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(id);

                if (data == null)
                {
                    WriteInteger(0);
                    WriteString("Unknown Error");
                }
                else
                {
                    WriteInteger(data.Id);
                    WriteString(data.Username);
                }
            }
        }
    }
}