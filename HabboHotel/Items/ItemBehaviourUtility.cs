namespace Plus.HabboHotel.Items
{
    using System;
    using Communication.Packets.Outgoing;
    using Data.Toner;
    using Groups;

    internal static class ItemBehaviourUtility
    {
        public static void GenerateExtradata(Item item, ServerPacket message)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                default:
                    message.WriteInteger(1);
                    message.WriteInteger(0);
                    message.WriteString(item.GetBaseItem().InteractionType != InteractionType.FootballGate
                        ? item.ExtraData
                        : string.Empty);
                    break;
                case InteractionType.GnomeBox:
                    message.WriteInteger(0);
                    message.WriteInteger(0);
                    message.WriteString("");
                    break;
                case InteractionType.PetBreedingBox:
                case InteractionType.PurchasableClothing:
                    message.WriteInteger(0);
                    message.WriteInteger(0);
                    message.WriteString("0");
                    break;
                case InteractionType.Stacktool:
                    message.WriteInteger(0);
                    message.WriteInteger(0);
                    message.WriteString("");
                    break;
                case InteractionType.Wallpaper:
                    message.WriteInteger(2);
                    message.WriteInteger(0);
                    message.WriteString(item.ExtraData);
                    break;
                case InteractionType.Floor:
                    message.WriteInteger(3);
                    message.WriteInteger(0);
                    message.WriteString(item.ExtraData);
                    break;
                case InteractionType.Landscape:
                    message.WriteInteger(4);
                    message.WriteInteger(0);
                    message.WriteString(item.ExtraData);
                    break;
                case InteractionType.GuildItem:
                case InteractionType.GuildGate:
                case InteractionType.GuildForum:
                    Group group = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(item.GroupId, out group))
                    {
                        message.WriteInteger(1);
                        message.WriteInteger(0);
                        message.WriteString(item.ExtraData);
                    }
                    else
                    {
                        message.WriteInteger(0);
                        message.WriteInteger(2);
                        message.WriteInteger(5);
                        message.WriteString(item.ExtraData);
                        message.WriteString(group.Id.ToString());
                        message.WriteString(group.Badge);
                        message.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour1, true));
                        message.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour2, false));
                    }
                    break;
                case InteractionType.Background:
                    message.WriteInteger(0);
                    message.WriteInteger(1);
                    if (!string.IsNullOrEmpty(item.ExtraData))
                    {
                        message.WriteInteger(item.ExtraData.Split(Convert.ToChar(9)).Length / 2);
                        for (var i = 0; i <= item.ExtraData.Split(Convert.ToChar(9)).Length - 1; i++)
                        {
                            message.WriteString(item.ExtraData.Split(Convert.ToChar(9))[i]);
                        }
                    }
                    else
                    {
                        message.WriteInteger(0);
                    }

                    break;
                case InteractionType.Gift:
                {
                    var extraData = item.ExtraData.Split(Convert.ToChar(5));
                    if (extraData.Length != 7)
                    {
                        message.WriteInteger(0);
                        message.WriteInteger(0);
                        message.WriteString(item.ExtraData);
                    }
                    else
                    {
                        var style = int.Parse(extraData[6]) * 1000 + int.Parse(extraData[6]);
                        var purchaser = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(extraData[2]));
                        if (purchaser == null)
                        {
                            message.WriteInteger(0);
                            message.WriteInteger(0);
                            message.WriteString(item.ExtraData);
                        }
                        else
                        {
                            message.WriteInteger(style);
                            message.WriteInteger(1);
                            message.WriteInteger(6);
                            message.WriteString("EXTRA_PARAM");
                            message.WriteString("");
                            message.WriteString("MESSAGE");
                            message.WriteString(extraData[1]);
                            message.WriteString("PURCHASER_NAME");
                            message.WriteString(purchaser.Username);
                            message.WriteString("PURCHASER_FIGURE");
                            message.WriteString(purchaser.Look);
                            message.WriteString("PRODUCT_CODE");
                            message.WriteString("A1 KUMIANKKA");
                            message.WriteString("state");
                            message.WriteString(item.MagicRemove ? "1" : "0");
                        }
                    }
                }
                    break;
                case InteractionType.Mannequin:
                    message.WriteInteger(0);
                    message.WriteInteger(1);
                    message.WriteInteger(3);
                    if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        var stuff = item.ExtraData.Split(Convert.ToChar(5));
                        message.WriteString("GENDER");
                        message.WriteString(stuff[0]);
                        message.WriteString("FIGURE");
                        message.WriteString(stuff[1]);
                        message.WriteString("OUTFIT_NAME");
                        message.WriteString(stuff[2]);
                    }
                    else
                    {
                        message.WriteString("GENDER");
                        message.WriteString("");
                        message.WriteString("FIGURE");
                        message.WriteString("");
                        message.WriteString("OUTFIT_NAME");
                        message.WriteString("");
                    }
                    break;
                case InteractionType.Toner:
                    if (item.RoomId != 0)
                    {
                        if (item.GetRoom().TonerData == null)
                        {
                            item.GetRoom().TonerData = new TonerData(item.Id);
                        }
                        message.WriteInteger(0);
                        message.WriteInteger(5);
                        message.WriteInteger(4);
                        message.WriteInteger(item.GetRoom().TonerData.Enabled);
                        message.WriteInteger(item.GetRoom().TonerData.Hue);
                        message.WriteInteger(item.GetRoom().TonerData.Saturation);
                        message.WriteInteger(item.GetRoom().TonerData.Lightness);
                    }
                    else
                    {
                        message.WriteInteger(0);
                        message.WriteInteger(0);
                        message.WriteString(string.Empty);
                    }
                    break;
                case InteractionType.BadgeDisplay:
                    message.WriteInteger(0);
                    message.WriteInteger(2);
                    message.WriteInteger(4);
                    var badgeData = item.ExtraData.Split(Convert.ToChar(9));
                    if (item.ExtraData.Contains(Convert.ToChar(9).ToString()))
                    {
                        message.WriteString("0"); //No idea
                        message.WriteString(badgeData[0]); //Badge name
                        message.WriteString(badgeData[1]); //Owner
                        message.WriteString(badgeData[2]); //Date
                    }
                    else
                    {
                        message.WriteString("0"); //No idea
                        message.WriteString("DEV"); //Badge name
                        message.WriteString("Sledmore"); //Owner
                        message.WriteString("13-13-1337"); //Date
                    }
                    break;
                case InteractionType.Television:
                    message.WriteInteger(0);
                    message.WriteInteger(1);
                    message.WriteInteger(1);
                    message.WriteString("THUMBNAIL_URL");

                    //Message.WriteString("http://img.youtube.com/vi/" + PlusEnvironment.GetGame().GetTelevisionManager().TelevisionList.OrderBy(x => Guid.NewGuid()).FirstOrDefault().YouTubeId + "/3.jpg");
                    message.WriteString("");
                    break;
                case InteractionType.Lovelock:
                    if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        var eData = item.ExtraData.Split((char) 5);
                        var I = 0;
                        message.WriteInteger(0);
                        message.WriteInteger(2);
                        message.WriteInteger(eData.Length);
                        while (I < eData.Length)
                        {
                            message.WriteString(eData[I]);
                            I++;
                        }
                    }
                    else
                    {
                        message.WriteInteger(0);
                        message.WriteInteger(0);
                        message.WriteString("0");
                    }

                    break;
                case InteractionType.MonsterplantSeed:
                    message.WriteInteger(0);
                    message.WriteInteger(1);
                    message.WriteInteger(1);
                    message.WriteString("rarity");
                    message.WriteString("1"); //Leve should be dynamic.
                    break;
            }
        }

        public static void GenerateWallExtradata(Item item, ServerPacket message)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                default:
                    message.WriteString(item.ExtraData);
                    break;
                case InteractionType.Postit:
                    message.WriteString(item.ExtraData.Split(' ')[0]);
                    break;
            }
        }
    }
}