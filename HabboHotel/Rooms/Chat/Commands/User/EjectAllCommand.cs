namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using System.Linq;
    using GameClients;

    internal class EjectAllCommand : IChatCommand
    {
        public string PermissionRequired => "command_ejectall";

        public string Parameters => "";

        public string Description => "Removes all of the items from the room.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (session.GetHabbo().Id == room.OwnerId)
            {
                //Let us check anyway.
                if (!room.CheckRights(session, true))
                {
                    return;
                }

                foreach (var item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
                {
                    if (item == null || item.UserId == session.GetHabbo().Id)
                    {
                        continue;
                    }

                    var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(item.UserId);
                    if (targetClient != null && targetClient.GetHabbo() != null)
                    {
                        room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                        targetClient.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo,
                                item.LimitedTot);
                        targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                    else
                    {
                        room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id + "' LIMIT 1");
                        }
                    }
                }
            }
            else
            {
                foreach (var item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
                {
                    if (item == null || item.UserId != session.GetHabbo().Id)
                    {
                        continue;
                    }

                    var targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(item.UserId);
                    if (targetClient != null && targetClient.GetHabbo() != null)
                    {
                        room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                        targetClient.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo,
                                item.LimitedTot);
                        targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                    else
                    {
                        room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id + "' LIMIT 1");
                        }
                    }
                }
            }
        }
    }
}