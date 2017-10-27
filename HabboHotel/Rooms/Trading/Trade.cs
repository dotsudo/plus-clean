namespace Plus.HabboHotel.Rooms.Trading
{
    using System.Linq;
    using Communication.Packets.Outgoing;
    using Communication.Packets.Outgoing.Inventory.Furni;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using Communication.Packets.Outgoing.Inventory.Trading;
    using Communication.Packets.Outgoing.Moderation;
    using Items;

    public sealed class Trade
    {
        private readonly Room Instance;

        public Trade(int Id, RoomUser Player1, RoomUser Player2, Room Instance)
        {
            this.Id = Id;
            CanChange = true;
            this.Instance = Instance;
            Users = new TradeUser[2];
            Users[0] = new TradeUser(Player1);
            Users[1] = new TradeUser(Player2);
            Player1.IsTrading = true;
            Player1.TradeId = this.Id;
            Player1.TradePartner = Player2.UserId;
            Player2.IsTrading = true;
            Player2.TradeId = this.Id;
            Player2.TradePartner = Player1.UserId;
        }

        public int Id { get; set; }
        public TradeUser[] Users { get; set; }
        public bool CanChange { get; set; }

        public bool AllAccepted
        {
            get
            {
                foreach (var User in Users)
                {
                    if (User == null)
                    {
                        continue;
                    }

                    if (!User.HasAccepted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void SendPacket(ServerPacket Packet)
        {
            foreach (var TradeUser in Users)
            {
                if (TradeUser == null || TradeUser.RoomUser == null || TradeUser.RoomUser.GetClient() == null)
                {
                    continue;
                }

                TradeUser.RoomUser.GetClient().SendPacket(Packet);
            }
        }

        public void RemoveAccepted()
        {
            foreach (var User in Users)
            {
                if (User == null)
                {
                    continue;
                }

                User.HasAccepted = false;
            }
        }

        public void EndTrade(int UserId)
        {
            foreach (var TradeUser in Users)
            {
                if (TradeUser == null || TradeUser.RoomUser == null)
                {
                    continue;
                }

                RemoveTrade(TradeUser.RoomUser.UserId);
            }

            SendPacket(new TradingClosedComposer(UserId));
            Instance.GetTrading().RemoveTrade(Id);
        }

        public void Finish()
        {
            foreach (var TradeUser in Users)
            {
                if (TradeUser == null)
                {
                    continue;
                }

                RemoveTrade(TradeUser.RoomUser.UserId);
            }

            ProcessItems();
            SendPacket(new TradingFinishComposer());
            Instance.GetTrading().RemoveTrade(Id);
        }

        public void RemoveTrade(int UserId)
        {
            var TradeUser = Users[0];
            if (TradeUser.RoomUser.UserId != UserId)
            {
                TradeUser = Users[1];
            }
            TradeUser.RoomUser.RemoveStatus("trd");
            TradeUser.RoomUser.UpdateNeeded = true;
            TradeUser.RoomUser.IsTrading = false;
            TradeUser.RoomUser.TradeId = 0;
            TradeUser.RoomUser.TradePartner = 0;
        }

        public void ProcessItems()
        {
            var UserOne = Users[0].OfferedItems.Values.ToList();
            var UserTwo = Users[1].OfferedItems.Values.ToList();
            var RoomUserOne = Users[0].RoomUser;
            var RoomUserTwo = Users[1].RoomUser;
            var logUserOne = "";
            var logUserTwo = "";
            if (RoomUserOne == null ||
                RoomUserOne.GetClient() == null ||
                RoomUserOne.GetClient().GetHabbo() == null ||
                RoomUserOne.GetClient().GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }
            if (RoomUserTwo == null ||
                RoomUserTwo.GetClient() == null ||
                RoomUserTwo.GetClient().GetHabbo() == null ||
                RoomUserTwo.GetClient().GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }

            foreach (var Item in UserOne)
            {
                var I = RoomUserOne.GetClient().GetHabbo().GetInventoryComponent().GetItem(Item.Id);
                if (I == null)
                {
                    SendPacket(new BroadcastMessageAlertComposer("Error! Trading Failed!"));
                    return;
                }
            }
            foreach (var Item in UserTwo)
            {
                var I = RoomUserTwo.GetClient().GetHabbo().GetInventoryComponent().GetItem(Item.Id);
                if (I == null)
                {
                    SendPacket(new BroadcastMessageAlertComposer("Error! Trading Failed!"));
                    return;
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (var Item in UserOne)
                {
                    logUserOne += Item.Id + ";";
                    RoomUserOne.GetClient().GetHabbo().GetInventoryComponent().RemoveItem(Item.Id);
                    if (Item.Data.InteractionType == InteractionType.Exchange &&
                        PlusEnvironment.GetSettingsManager().TryGetValue("trading.auto_exchange_redeemables") == "1")
                    {
                        RoomUserTwo.GetClient().GetHabbo().Credits += Item.Data.BehaviourData;
                        RoomUserTwo.GetClient().SendPacket(new CreditBalanceComposer(RoomUserTwo.GetClient().GetHabbo().Credits));
                        dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", Item.Id);
                        dbClient.RunQuery();
                    }
                    else
                    {
                        if (RoomUserTwo.GetClient().GetHabbo().GetInventoryComponent().TryAddItem(Item))
                        {
                            RoomUserTwo.GetClient().SendPacket(new FurniListAddComposer(Item));
                            RoomUserTwo.GetClient().SendPacket(new FurniListNotificationComposer(Item.Id, 1));
                            dbClient.SetQuery("UPDATE `items` SET `user_id` = @user WHERE id=@id LIMIT 1");
                            dbClient.AddParameter("user", RoomUserTwo.UserId);
                            dbClient.AddParameter("id", Item.Id);
                            dbClient.RunQuery();
                        }
                    }
                }
                foreach (var Item in UserTwo)
                {
                    logUserTwo += Item.Id + ";";
                    RoomUserTwo.GetClient().GetHabbo().GetInventoryComponent().RemoveItem(Item.Id);
                    if (Item.Data.InteractionType == InteractionType.Exchange &&
                        PlusEnvironment.GetSettingsManager().TryGetValue("trading.auto_exchange_redeemables") == "1")
                    {
                        RoomUserOne.GetClient().GetHabbo().Credits += Item.Data.BehaviourData;
                        RoomUserOne.GetClient().SendPacket(new CreditBalanceComposer(RoomUserOne.GetClient().GetHabbo().Credits));
                        dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", Item.Id);
                        dbClient.RunQuery();
                    }
                    else
                    {
                        if (RoomUserOne.GetClient().GetHabbo().GetInventoryComponent().TryAddItem(Item))
                        {
                            RoomUserOne.GetClient().SendPacket(new FurniListAddComposer(Item));
                            RoomUserOne.GetClient().SendPacket(new FurniListNotificationComposer(Item.Id, 1));
                            dbClient.SetQuery("UPDATE `items` SET `user_id` = @user WHERE id=@id LIMIT 1");
                            dbClient.AddParameter("user", RoomUserOne.UserId);
                            dbClient.AddParameter("id", Item.Id);
                            dbClient.RunQuery();
                        }
                    }
                }

                dbClient.SetQuery("INSERT INTO `logs_client_trade` VALUES(null, @1id, @2id, @1items, @2items, UNIX_TIMESTAMP())");
                dbClient.AddParameter("1id", RoomUserOne.UserId);
                dbClient.AddParameter("2id", RoomUserTwo.UserId);
                dbClient.AddParameter("1items", logUserOne);
                dbClient.AddParameter("2items", logUserTwo);
                dbClient.RunQuery();
            }
        }
    }
}