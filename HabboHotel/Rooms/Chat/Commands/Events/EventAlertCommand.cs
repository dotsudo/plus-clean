﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Events
{
    using System;
    using Communication.Packets.Outgoing.Moderation;
    using GameClients;

    internal class EventAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_event_alert";
        public string Parameters => "";
        public string Description => "Send a hotel alert for your event!";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session != null)
            {
                if (Room != null)
                {
                    if (Params.Length != 1)
                    {
                        Session.SendWhisper("Invalid command! :eventalert");
                    }
                    else if (!PlusEnvironment.Event)
                    {
                        PlusEnvironment.GetGame()
                            .GetClientManager()
                            .SendPacket(new BroadcastMessageAlertComposer(
                                ":follow " + Session.GetHabbo().Username + " for events! win prizes!\r\n- " +
                                Session.GetHabbo().Username));
                        PlusEnvironment.lastEvent = DateTime.Now;
                        PlusEnvironment.Event = true;
                    }
                    else
                    {
                        var timeSpan = DateTime.Now - PlusEnvironment.lastEvent;
                        if (timeSpan.Hours >= 1)
                        {
                            PlusEnvironment.GetGame()
                                .GetClientManager()
                                .SendPacket(new BroadcastMessageAlertComposer(
                                    ":follow " + Session.GetHabbo().Username + " for events! win prizes!\r\n- " +
                                    Session.GetHabbo().Username));
                            PlusEnvironment.lastEvent = DateTime.Now;
                        }
                        else
                        {
                            var num = checked(60 - timeSpan.Minutes);
                            Session.SendWhisper("Event Cooldown! " + num + " minutes left until another event can be hosted.");
                        }
                    }
                }
            }
        }
    }
}