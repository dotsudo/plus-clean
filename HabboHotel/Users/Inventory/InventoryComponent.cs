namespace Plus.HabboHotel.Users.Inventory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Bots;
    using Communication.Packets.Outgoing.Inventory.Furni;
    using GameClients;
    using Items;
    using Pets;
    using Rooms.AI;

    public class InventoryComponent
    {
        private readonly ConcurrentDictionary<int, Bot> _botItems;
        private readonly ConcurrentDictionary<int, Item> _floorItems;
        private readonly ConcurrentDictionary<int, Pet> _petsItems;
        private readonly int _userId;
        private readonly ConcurrentDictionary<int, Item> _wallItems;
        private GameClient _client;

        public InventoryComponent(int userId, GameClient client)
        {
            _client = client;
            _userId = userId;
            _floorItems = new ConcurrentDictionary<int, Item>();
            _wallItems = new ConcurrentDictionary<int, Item>();
            _petsItems = new ConcurrentDictionary<int, Pet>();
            _botItems = new ConcurrentDictionary<int, Bot>();
            Init();
        }

        internal IEnumerable<Item> GetItems => _floorItems.Values.Concat(_wallItems.Values);

        internal IEnumerable<Item> GetWallAndFloor => _floorItems.Values.Concat(_wallItems.Values);

        private void Init()
        {
            if (_floorItems.Count > 0)
            {
                _floorItems.Clear();
            }
            if (_wallItems.Count > 0)
            {
                _wallItems.Clear();
            }
            if (_petsItems.Count > 0)
            {
                _petsItems.Clear();
            }
            if (_botItems.Count > 0)
            {
                _botItems.Clear();
            }
            var items = ItemLoader.GetItemsForUser(_userId);
            foreach (var item in items.ToList())
            {
                if (item.IsFloorItem)
                {
                    if (!_floorItems.TryAdd(item.Id, item))
                    {
                    }
                }
                else if (item.IsWallItem)
                {
                    if (!_wallItems.TryAdd(item.Id, item))
                    {
                    }
                }
            }

            var pets = PetLoader.GetPetsForUser(Convert.ToInt32(_userId));
            foreach (var pet in pets)
            {
                if (!_petsItems.TryAdd(pet.PetId, pet))
                {
                    Console.WriteLine("Error whilst loading pet x1: " + pet.PetId);
                }
            }

            var bots = BotLoader.GetBotsForUser(Convert.ToInt32(_userId));
            foreach (var bot in bots)
            {
                if (!_botItems.TryAdd(bot.Id, bot))
                {
                    Console.WriteLine("Error whilst loading bot x1: " + bot.Id);
                }
            }
        }

        public void ClearItems()
        {
            UpdateItems(true);
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM items WHERE room_id='0' AND user_id = " + _userId); //Do join 
            }
            _floorItems.Clear();
            _wallItems.Clear();
            _client?.SendPacket(new FurniListUpdateComposer());
        }

        public void SetIdleState()
        {
            if (_botItems != null)
            {
                _botItems.Clear();
            }
            if (_petsItems != null)
            {
                _petsItems.Clear();
            }
            if (_floorItems != null)
            {
                _floorItems.Clear();
            }
            if (_wallItems != null)
            {
                _wallItems.Clear();
            }
            _client = null;
        }

        internal void UpdateItems(bool fromDatabase)
        {
            if (fromDatabase)
            {
                Init();
            }
            _client?.SendPacket(new FurniListUpdateComposer());
        }

        public Item GetItem(int id)
        {
            if (_floorItems.ContainsKey(id))
            {
                return _floorItems[id];
            }

            return _wallItems.ContainsKey(id) ? _wallItems[id] : null;
        }

        public Item AddNewItem(int id, int baseItem, string extraData, int group, bool toInsert, bool fromRoom, int limitedNumber,
                               int limitedStack)
        {
            if (toInsert)
            {
                if (fromRoom)
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `items` SET `room_id` = '0', `user_id` = '" + _userId + "' WHERE `id` = '" +
                                          id + "' LIMIT 1");
                    }
                }
                else
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        if (id > 0)
                        {
                            dbClient.RunQuery(
                                "INSERT INTO `items` (`id`,`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" +
                                id +
                                "', '" +
                                baseItem +
                                "', '" +
                                _userId +
                                "', '" +
                                limitedNumber +
                                "', '" +
                                limitedStack +
                                "')");
                        }
                        else
                        {
                            dbClient.SetQuery(
                                "INSERT INTO `items` (`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" +
                                baseItem +
                                "', '" +
                                _userId +
                                "', '" +
                                limitedNumber +
                                "', '" +
                                limitedStack +
                                "')");
                            id = Convert.ToInt32(dbClient.InsertQuery());
                        }
                        SendNewItems(Convert.ToInt32(id));
                        if (group > 0)
                        {
                            dbClient.RunQuery("INSERT INTO `items_groups` VALUES (" + id + ", " + group + ")");
                        }
                        if (!string.IsNullOrEmpty(extraData))
                        {
                            dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extradata WHERE `id` = '" + id + "' LIMIT 1");
                            dbClient.AddParameter("extradata", extraData);
                            dbClient.RunQuery();
                        }
                    }
                }
            }
            var itemToAdd = new Item(id, 0, baseItem, extraData, 0, 0, 0, 0, _userId, group, limitedNumber, limitedStack,
                string.Empty);
            if (UserHoldsItem(id))
            {
                RemoveItem(id);
            }
            if (itemToAdd.IsWallItem)
            {
                _wallItems.TryAdd(itemToAdd.Id, itemToAdd);
            }
            else
            {
                _floorItems.TryAdd(itemToAdd.Id, itemToAdd);
            }
            return itemToAdd;
        }

        private bool UserHoldsItem(int itemId)
        {
            if (_floorItems.ContainsKey(itemId))
            {
                return true;
            }
            if (_wallItems.ContainsKey(itemId))
            {
                return true;
            }

            return false;
        }

        public void RemoveItem(int id)
        {
            if (GetClient() == null)
            {
                return;
            }

            if (GetClient().GetHabbo() == null || GetClient().GetHabbo().GetInventoryComponent() == null)
            {
                GetClient().Disconnect();
            }
            if (_floorItems.ContainsKey(id))
            {
                Item toRemove = null;
                _floorItems.TryRemove(id, out toRemove);
            }
            if (_wallItems.ContainsKey(id))
            {
                Item toRemove = null;
                _wallItems.TryRemove(id, out toRemove);
            }
            GetClient().SendPacket(new FurniListRemoveComposer(id));
        }

        private GameClient GetClient() => PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(_userId);

        public void SendNewItems(int id)
        {
            _client.SendPacket(new FurniListNotificationComposer(id, 1));
        }

        public ICollection<Pet> GetPets() => _petsItems.Values;

        public bool TryAddPet(Pet pet)
        {
            //TODO: Sort this mess.
            pet.RoomId = 0;
            pet.PlacedInRoom = false;
            return _petsItems.TryAdd(pet.PetId, pet);
        }

        public bool TryRemovePet(int petId, out Pet petItem)
        {
            if (_petsItems.ContainsKey(petId))
            {
                return _petsItems.TryRemove(petId, out petItem);
            }

            petItem = null;
            return false;
        }

        public bool TryGetPet(int petId, out Pet pet)
        {
            if (_petsItems.ContainsKey(petId))
            {
                return _petsItems.TryGetValue(petId, out pet);
            }

            pet = null;
            return false;
        }

        public ICollection<Bot> GetBots() => _botItems.Values;

        public bool TryAddBot(Bot bot) => _botItems.TryAdd(bot.Id, bot);

        public bool TryRemoveBot(int botId, out Bot bot)
        {
            if (_botItems.ContainsKey(botId))
            {
                return _botItems.TryRemove(botId, out bot);
            }

            bot = null;
            return false;
        }

        public bool TryGetBot(int botId, out Bot bot)
        {
            if (_botItems.ContainsKey(botId))
            {
                return _botItems.TryGetValue(botId, out bot);
            }

            bot = null;
            return false;
        }

        public bool TryAddItem(Item item)
        {
            if (item.Data.Type.ToString().ToLower() == "s") // ItemType.FLOOR)
            {
                return _floorItems.TryAdd(item.Id, item);
            }
            if (item.Data.Type.ToString().ToLower() == "i") //ItemType.WALL)
            {
                return _wallItems.TryAdd(item.Id, item);
            }

            throw new InvalidOperationException("Item did not match neither floor or wall item");
        }

        public bool TryAddFloorItem(int itemId, Item item) => _floorItems.TryAdd(itemId, item);

        public bool TryAddWallItem(int itemId, Item item) => _floorItems.TryAdd(itemId, item);

        public ICollection<Item> GetFloorItems() => _floorItems.Values;

        public ICollection<Item> GetWallItems() => _wallItems.Values;
    }
}