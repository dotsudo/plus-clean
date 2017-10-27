namespace Plus.Communication.Packets.Incoming.Rooms.Furni.LoveLocks
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Rooms.Furni.LoveLocks;

    internal class ConfirmLoveLockEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var pId = packet.PopInt();
            var isConfirmed = packet.PopBoolean();

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var item = room.GetRoomItemHandler().GetItem(pId);

            if (item?.GetBaseItem() == null || item.GetBaseItem().InteractionType != InteractionType.Lovelock)
            {
                return;
            }

            var userOneId = item.InteractingUser;
            var userTwoId = item.InteractingUser2;

            var userOne = room.GetRoomUserManager().GetRoomUserByHabbo(userOneId);
            var userTwo = room.GetRoomUserManager().GetRoomUserByHabbo(userTwoId);

            if (userOne == null && userTwo == null)
            {
                item.InteractingUser = 0;
                item.InteractingUser2 = 0;
                session.SendNotification("Your partner has left the room or has cancelled the love lock.");
            }
            else if (userOne != null && (userOne.GetClient() == null || userTwo.GetClient() == null))
            {
                item.InteractingUser = 0;
                item.InteractingUser2 = 0;
                session.SendNotification("Your partner has left the room or has cancelled the love lock.");
            }
            else if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
            {
                userTwo.CanWalk = true;
                userTwo.GetClient().SendNotification("It appears this love lock has already been locked.");
                userTwo.LLPartner = 0;

                userOne.CanWalk = true;
                userOne.GetClient().SendNotification("It appears this love lock has already been locked.");
                userOne.LLPartner = 0;

                item.InteractingUser = 0;
                item.InteractingUser2 = 0;
            }
            else if (!isConfirmed)
            {
                item.InteractingUser = 0;
                item.InteractingUser2 = 0;

                userOne.LLPartner = 0;
                userTwo.LLPartner = 0;

                userOne.CanWalk = true;
                userTwo.CanWalk = true;
            }
            else
            {
                if (userOneId == session.GetHabbo().Id)
                {
                    session.SendPacket(new LoveLockDialogueSetLockedMessageComposer(pId));
                    userOne.LLPartner = userTwoId;
                }
                else if (userTwoId == session.GetHabbo().Id)
                {
                    session.SendPacket(new LoveLockDialogueSetLockedMessageComposer(pId));
                    userTwo.LLPartner = userOneId;
                }

                if (userOne.LLPartner == 0 || userTwo.LLPartner == 0)
                {
                }
                else
                {
                    item.ExtraData = "1" + (char) 5 + userOne.GetUsername() + (char) 5 + userTwo.GetUsername() + (char) 5 + userOne.GetClient().GetHabbo().Look + (char) 5 +
                                     userTwo.GetClient().GetHabbo().Look + (char) 5 + DateTime.Now.ToString("dd/MM/yyyy");

                    item.InteractingUser = 0;
                    item.InteractingUser2 = 0;

                    userOne.LLPartner = 0;
                    userTwo.LLPartner = 0;

                    item.UpdateState(true, true);

                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extraData WHERE `id` = @ID LIMIT 1");
                        dbClient.AddParameter("extraData", item.ExtraData);
                        dbClient.AddParameter("ID", item.Id);
                        dbClient.RunQuery();
                    }

                    userOne.GetClient().SendPacket(new LoveLockDialogueCloseMessageComposer(pId));
                    userTwo.GetClient().SendPacket(new LoveLockDialogueCloseMessageComposer(pId));

                    userOne.CanWalk = true;
                    userTwo.CanWalk = true;
                }
            }
        }
    }
}