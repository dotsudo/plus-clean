namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HabboHotel.Rooms;

    internal class UserUpdateComposer : ServerPacket
    {
        public UserUpdateComposer(ICollection<RoomUser> roomUsers)
            : base(ServerPacketHeader.UserUpdateMessageComposer)
        {
            WriteInteger(roomUsers.Count);
            foreach (var user in roomUsers.ToList())
            {
                WriteInteger(user.VirtualId);
                WriteInteger(user.X);
                WriteInteger(user.Y);
                WriteString(user.Z.ToString("0.00"));
                WriteInteger(user.RotHead);
                WriteInteger(user.RotBody);

                var statusComposer = new StringBuilder();
                statusComposer.Append("/");

                foreach (var status in user.Statusses.ToList())
                {
                    statusComposer.Append(status.Key);

                    if (!string.IsNullOrEmpty(status.Value))
                    {
                        statusComposer.Append(" ");
                        statusComposer.Append(status.Value);
                    }

                    statusComposer.Append("/");
                }

                statusComposer.Append("/");
                WriteString(statusComposer.ToString());
            }
        }
    }
}