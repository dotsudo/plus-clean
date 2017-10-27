namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;

    internal class SetMannequinFigureEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(session, true))
            {
                return;
            }

            var itemId = packet.PopInt();
            var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            var gender = session.GetHabbo().Gender.ToLower();
            var figure = "";

            foreach (var str in session.GetHabbo().Look.Split('.'))
            {
                if (str.Contains("hr") || str.Contains("hd") || str.Contains("he") || str.Contains("ea") || str.Contains("ha"))
                {
                    continue;
                }

                figure += str + ".";
            }

            figure = figure.TrimEnd('.');
            if (item.ExtraData.Contains(Convert.ToChar(5)))
            {
                var flags = item.ExtraData.Split(Convert.ToChar(5));
                item.ExtraData = gender + Convert.ToChar(5) + figure + Convert.ToChar(5) + flags[2];
            }
            else
            {
                item.ExtraData = gender + Convert.ToChar(5) + figure + Convert.ToChar(5) + "Default";
            }

            item.UpdateState(true, true);
        }
    }
}