namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger;

    internal class FriendListUpdateComposer : ServerPacket
    {
        public FriendListUpdateComposer(int friendId)
            : base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            WriteInteger(0); //Category Count
            WriteInteger(1); //Updates Count
            WriteInteger(-1); //Update
            WriteInteger(friendId);
        }

        public FriendListUpdateComposer(GameClient session, MessengerBuddy buddy)
            : base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            WriteInteger(0); //Category Count
            WriteInteger(1); //Updates Count
            WriteInteger(0); //Update

            var relationship = session.GetHabbo().Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(buddy.UserId)).Value;
            var y = relationship?.Type ?? 0;

            WriteInteger(buddy.UserId);
            WriteString(buddy.mUsername);
            WriteInteger(1);
            if (!buddy.mAppearOffline || session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                WriteBoolean(buddy.IsOnline);
            }
            else
            {
                WriteBoolean(false);
            }

            if (!buddy.mHideInroom || session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                WriteBoolean(buddy.InRoom);
            }
            else
            {
                WriteBoolean(false);
            }

            WriteString(""); //Habbo.IsOnline ? Habbo.Look : "");
            WriteInteger(0); // categoryid
            WriteString(buddy.mMotto);
            WriteString(string.Empty); // Facebook username
            WriteString(string.Empty);
            WriteBoolean(true); // Allows offline messaging
            WriteBoolean(false); // ?
            WriteBoolean(false); // Uses phone
            WriteShort(y);
        }
    }
}