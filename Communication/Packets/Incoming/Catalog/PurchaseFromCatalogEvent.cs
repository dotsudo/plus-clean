namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using HabboHotel.Badges;
    using HabboHotel.Catalog;
    using HabboHotel.Catalog.Utilities;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Users.Effects;
    using Outgoing.Catalog;
    using Outgoing.Inventory.AvatarEffects;
    using Outgoing.Inventory.Bots;
    using Outgoing.Inventory.Furni;
    using Outgoing.Inventory.Pets;
    using Outgoing.Inventory.Purse;
    using Outgoing.Moderation;

    internal class PurchaseFromCatalogEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (PlusEnvironment.GetSettingsManager().TryGetValue("catalog.enabled") != "1")
            {
                session.SendNotification("The hotel managers have disabled the catalogue");
                return;
            }

            var pageId = packet.PopInt();
            var itemId = packet.PopInt();
            var extraData = packet.PopString();
            var amount = packet.PopInt();

            CatalogPage page;
            if (!PlusEnvironment.GetGame().GetCatalog().TryGetPage(pageId, out page))
            {
                return;
            }

            if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumRank > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
            {
                return;
            }

            if (!page.Items.TryGetValue(itemId, out var item))
            {
                if (page.ItemOffers.ContainsKey(itemId))
                {
                    item = page.ItemOffers[itemId];
                    if (item == null)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            if (amount < 1 || amount > 100 || !item.HaveOffer)
            {
                amount = 1;
            }

            var amountPurchase = item.Amount > 1 ? item.Amount : amount;

            var totalCreditsCost = amount > 1 ? item.CostCredits * amount - (int) Math.Floor((double) amount / 6) * item.CostCredits : item.CostCredits;
            var totalPixelCost = amount > 1 ? item.CostPixels * amount - (int) Math.Floor((double) amount / 6) * item.CostPixels : item.CostPixels;
            var totalDiamondCost = amount > 1 ? item.CostDiamonds * amount - (int) Math.Floor((double) amount / 6) * item.CostDiamonds : item.CostDiamonds;

            if (session.GetHabbo().Credits < totalCreditsCost || session.GetHabbo().Duckets < totalPixelCost || session.GetHabbo().Diamonds < totalDiamondCost)
            {
                return;
            }

            var limitedEditionSells = 0;
            var limitedEditionStack = 0;

            switch (item.Data.InteractionType)
            {
                case InteractionType.None:
                    extraData = "";
                    break;

                case InteractionType.GuildItem:
                case InteractionType.GuildGate:
                    break;

                #region Pet handling

                case InteractionType.Pet:
                    try
                    {
                        var bits = extraData.Split('\n');
                        var petName = bits[0];
                        var race = bits[1];
                        var color = bits[2];

                        int.Parse(race); // to trigger any possible errors

                        if (!PetUtility.CheckPetName(petName))
                        {
                            return;
                        }

                        if (race.Length > 2)
                        {
                            return;
                        }

                        if (color.Length != 6)
                        {
                            return;
                        }

                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_PetLover", 1);
                    }
                    catch (Exception e)
                    {
                        ExceptionLogger.LogException(e);
                        return;
                    }

                    break;

                #endregion

                case InteractionType.Floor:
                case InteractionType.Wallpaper:
                case InteractionType.Landscape:

                    double number = 0;

                    try
                    {
                        if (string.IsNullOrEmpty(extraData))
                        {
                            number = 0;
                        }
                        else
                        {
                            number = double.Parse(extraData, PlusEnvironment.CultureInfo);
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionLogger.LogException(e);
                    }

                    extraData = number.ToString().Replace(',', '.');
                    break; // maintain extra data // todo: validate

                case InteractionType.Postit:
                    extraData = "FFFF33";
                    break;

                case InteractionType.Moodlight:
                    extraData = "1,1,1,#000000,255";
                    break;

                case InteractionType.Trophy:
                    extraData = session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) +
                                extraData;
                    break;

                case InteractionType.Mannequin:
                    extraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";
                    break;

                case InteractionType.BadgeDisplay:
                    if (!session.GetHabbo().GetBadgeComponent().HasBadge(extraData))
                    {
                        session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                        return;
                    }

                    extraData = extraData + Convert.ToChar(9) + session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" +
                                DateTime.Now.Year;
                    break;

                case InteractionType.Badge:
                {
                    if (session.GetHabbo().GetBadgeComponent().HasBadge(item.Data.ItemName))
                    {
                        session.SendPacket(new PurchaseErrorComposer(1));
                        return;
                    }

                    break;
                }
                default:
                    extraData = "";
                    break;
            }
            
            if (item.IsLimited)
            {
                if (item.LimitedEditionStack <= item.LimitedEditionSells)
                {
                    session.SendNotification("This item has sold out!\n\n" + "Please note, you have not recieved another item (You have also not been charged for it!)");
                    session.SendPacket(new CatalogUpdatedComposer());
                    session.SendPacket(new PurchaseOkComposer());
                    return;
                }

                item.LimitedEditionSells++;
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `catalog_items` SET `limited_sells` = @limitSells WHERE `id` = @itemId LIMIT 1");
                    dbClient.AddParameter("limitSells", item.LimitedEditionSells);
                    dbClient.AddParameter("itemId", item.Id);
                    dbClient.RunQuery();

                    limitedEditionSells = item.LimitedEditionSells;
                    limitedEditionStack = item.LimitedEditionStack;
                }
            }

            if (item.CostCredits > 0)
            {
                session.GetHabbo().Credits -= totalCreditsCost;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }

            if (item.CostPixels > 0)
            {
                session.GetHabbo().Duckets -= totalPixelCost;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, session.GetHabbo().Duckets)); //Love you, Tom.
            }

            if (item.CostDiamonds > 0)
            {
                session.GetHabbo().Diamonds -= totalDiamondCost;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Diamonds, 0, 5));
            }

            Item newItem = null;
            switch (item.Data.Type.ToString().ToLower())
            {
                default:
                    var generatedGenericItems = new List<Item>();

                    switch (item.Data.InteractionType)
                    {
                        default:
                            if (amountPurchase > 1)
                            {
                                var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase);

                                if (items != null)
                                {
                                    generatedGenericItems.AddRange(items);
                                }
                            }
                            else
                            {
                                newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData, 0, limitedEditionSells, limitedEditionStack);

                                if (newItem != null)
                                {
                                    generatedGenericItems.Add(newItem);
                                }
                            }
                            break;

                        case InteractionType.GuildGate:
                        case InteractionType.GuildItem:
                        case InteractionType.GuildForum:
                            if (amountPurchase > 1)
                            {
                                var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase, Convert.ToInt32(extraData));

                                if (items != null)
                                {
                                    generatedGenericItems.AddRange(items);
                                }
                            }
                            else
                            {
                                newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData, Convert.ToInt32(extraData));

                                if (newItem != null)
                                {
                                    generatedGenericItems.Add(newItem);
                                }
                            }
                            break;

                        case InteractionType.Arrow:
                        case InteractionType.Teleport:
                            for (var i = 0; i < amountPurchase; i++)
                            {
                                var teleItems = ItemFactory.CreateTeleporterItems(item.Data, session.GetHabbo());

                                if (teleItems != null)
                                {
                                    generatedGenericItems.AddRange(teleItems);
                                }
                            }

                            break;

                        case InteractionType.Moodlight:
                        {
                            if (amountPurchase > 1)
                            {
                                var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase);

                                if (items != null)
                                {
                                    generatedGenericItems.AddRange(items);
                                    foreach (var I in items)
                                    {
                                        ItemFactory.CreateMoodlightData(I);
                                    }
                                }
                            }
                            else
                            {
                                newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData);

                                if (newItem != null)
                                {
                                    generatedGenericItems.Add(newItem);
                                    ItemFactory.CreateMoodlightData(newItem);
                                }
                            }
                        }

                            break;

                        case InteractionType.Toner:
                        {
                            if (amountPurchase > 1)
                            {
                                var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase);

                                if (items != null)
                                {
                                    generatedGenericItems.AddRange(items);
                                    foreach (var I in items)
                                    {
                                        ItemFactory.CreateTonerData(I);
                                    }
                                }
                            }
                            else
                            {
                                newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData);

                                if (newItem != null)
                                {
                                    generatedGenericItems.Add(newItem);
                                    ItemFactory.CreateTonerData(newItem);
                                }
                            }
                        }

                            break;

                        case InteractionType.Deal:
                        {
                            CatalogDeal deal = null;
                            if (PlusEnvironment.GetGame().GetCatalog().TryGetDeal(item.Data.BehaviourData, out deal))
                            {
                                foreach (var catalogItem in deal.ItemDataList.ToList())
                                {
                                    var items = ItemFactory.CreateMultipleItems(catalogItem.Data, session.GetHabbo(), "", amountPurchase);

                                    if (items != null)
                                    {
                                        generatedGenericItems.AddRange(items);
                                    }
                                }
                            }

                            break;
                        }

                        case InteractionType.Roomdeal:
                        {
                            CatalogDeal deal;
                            if (PlusEnvironment.GetGame().GetCatalog().TryGetDeal(item.Data.BehaviourData, out deal))
                            {
                                var room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(deal.RoomId);

                                if (room == null)
                                {
                                    session.SendNotification("There was an error loading this Room Bundle, if this happens again please contact hotel management!");
                                    return;
                                }

                                var newRoom = PlusEnvironment.GetGame().GetRoomManager().CreateRoom(session, room.Name, room.Description, room.ModelName, room.Category, 10,
                                    room.TradeSettings, room.Wallpaper, room.Floor, room.Landscape, room.WallThickness, room.FloorThickness);
                                if (newRoom == null)
                                {
                                    return;
                                }

                                var myRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(newRoom.Id);
                                if (myRoom != null)
                                {
                                    var items = ItemLoader.GetItemsForRoom(deal.RoomId, room);
                                    Item teleLink = null;

                                    foreach (var furni in items)
                                    {
                                        if (furni.Data.InteractionType == InteractionType.Teleport)
                                        {
                                            if (teleLink == null)
                                            {
                                                var teleItems = ItemFactory.CreateTeleporterItems(furni.Data, session.GetHabbo());

                                                if (teleItems != null && teleItems.ToList().Count > 1)
                                                {
                                                    myRoom.GetRoomItemHandler().SetFloorItem(session, teleItems[0], furni.GetX, furni.GetY, furni.Rotation, true, true, true, false,
                                                        furni.GetZ);
                                                    teleLink = teleItems[1];
                                                }
                                            }
                                            else
                                            {
                                                myRoom.GetRoomItemHandler().SetFloorItem(session, teleLink, furni.GetX, furni.GetY, furni.Rotation, true, true, true, false,
                                                    furni.GetZ);
                                                teleLink = null;
                                            }
                                        }
                                        else
                                        {
                                            newItem = ItemFactory.CreateSingleItemNullable(furni.Data, session.GetHabbo(), furni.ExtraData, furni.GroupId);

                                            if (newItem != null)
                                            {
                                                if (newItem.IsWallItem)
                                                {
                                                    newItem.WallCoord = furni.WallCoord;
                                                    myRoom.GetRoomItemHandler().SetWallItem(session, newItem);
                                                }
                                                else
                                                {
                                                    myRoom.GetRoomItemHandler().SetFloorItem(session, newItem, furni.GetX, furni.GetY, furni.Rotation, true, true, true, false,
                                                        furni.GetZ);
                                                }
                                            }
                                        }
                                    }

                                    session.GetHabbo().PrepareRoom(myRoom.Id, "");
                                }
                            }

                            break;
                        }
                    }

                    foreach (var purchasedItem in generatedGenericItems)
                    {
                        if (session.GetHabbo().GetInventoryComponent().TryAddItem(purchasedItem))
                        {
                            //Session.SendMessage(new FurniListAddComposer(PurchasedItem));
                            session.SendPacket(new FurniListNotificationComposer(purchasedItem.Id, 1));
                        }
                    }

                    break;

                case "e":
                    AvatarEffect effect = null;

                    if (session.GetHabbo().Effects().HasEffect(item.Data.SpriteId))
                    {
                        effect = session.GetHabbo().Effects().GetEffectNullable(item.Data.SpriteId);

                        if (effect != null)
                        {
                            effect.AddToQuantity();
                        }
                    }
                    else
                    {
                        effect = AvatarEffectFactory.CreateNullable(session.GetHabbo(), item.Data.SpriteId, 3600);
                    }

                    if (effect != null) // && Session.GetHabbo().Effects().TryAdd(Effect))
                    {
                        session.SendPacket(new AvatarEffectAddedComposer(item.Data.SpriteId, 3600));
                    }
                    break;

                case "r":
                    var bot = BotUtility.CreateBot(item.Data, session.GetHabbo().Id);
                    if (bot != null)
                    {
                        session.GetHabbo().GetInventoryComponent().TryAddBot(bot);
                        session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
                        session.SendPacket(new FurniListNotificationComposer(bot.Id, 5));
                    }
                    else
                    {
                        session.SendNotification("Oops! There was an error whilst purchasing this bot. It seems that there is no bot data for the bot!");
                    }
                    break;

                case "b":
                {
                    session.GetHabbo().GetBadgeComponent().GiveBadge(item.Data.ItemName, true, session);
                    session.SendPacket(new FurniListNotificationComposer(0, 4));
                    break;
                }

                case "p":
                {
                    var petData = extraData.Split('\n');

                    var generatedPet = PetUtility.CreatePet(session.GetHabbo().Id, petData[0], item.Data.BehaviourData, petData[1], petData[2]);
                    if (generatedPet != null)
                    {
                        session.GetHabbo().GetInventoryComponent().TryAddPet(generatedPet);

                        session.SendPacket(new FurniListNotificationComposer(generatedPet.PetId, 3));
                        session.SendPacket(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));

                        ItemData petFood = null;
                        if (PlusEnvironment.GetGame().GetItemManager().GetItem(320, out petFood))
                        {
                            var food = ItemFactory.CreateSingleItemNullable(petFood, session.GetHabbo(), "");
                            if (food != null)
                            {
                                session.GetHabbo().GetInventoryComponent().TryAddItem(food);
                                session.SendPacket(new FurniListNotificationComposer(food.Id, 1));
                            }
                        }
                    }
                    break;
                }
            }

            BadgeDefinition badge;

            if (!string.IsNullOrEmpty(item.Badge) &&
                PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(item.Badge, out badge) &&
                (string.IsNullOrEmpty(badge.RequiredRight) || session.GetHabbo().GetPermissions().HasRight(badge.RequiredRight)))
            {
                session.GetHabbo().GetBadgeComponent().GiveBadge(badge.Code, true, session);
            }

            session.SendPacket(new PurchaseOkComposer(item, item.Data));
            session.SendPacket(new FurniListUpdateComposer());
        }
    }
}