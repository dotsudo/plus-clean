namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using Communication.Packets.Outgoing.Rooms.Session;
    using GameClients;

    internal class SummonCommand : IChatCommand
    {
        public string PermissionRequired => "command_summon";

        public string Parameters => "%username%";

        public string Description => "Bring another user to your current room.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to summon.");
                return;
            }

            var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (targetClient.GetHabbo() == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Get a life.");
                return;
            }

            targetClient.SendNotification("You have been summoned to " + session.GetHabbo().Username + "!");
            if (!targetClient.GetHabbo().InRoom)
            {
                targetClient.SendPacket(new RoomForwardComposer(session.GetHabbo().CurrentRoomId));
            }
            else
            {
                targetClient.GetHabbo().PrepareRoom(session.GetHabbo().CurrentRoomId, "");
            }
        }
    }
}