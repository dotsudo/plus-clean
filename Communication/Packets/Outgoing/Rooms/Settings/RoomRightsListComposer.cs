namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using System.Linq;
    using HabboHotel.Rooms;

    internal class RoomRightsListComposer : ServerPacket
    {
        public RoomRightsListComposer(Room instance)
            : base(ServerPacketHeader.RoomRightsListMessageComposer)
        {
            WriteInteger(instance.Id);

            WriteInteger(instance.UsersWithRights.Count);
            foreach (var id in instance.UsersWithRights.ToList())
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