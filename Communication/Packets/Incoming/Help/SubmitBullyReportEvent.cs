﻿namespace Plus.Communication.Packets.Incoming.Help
{
    using System;
    using HabboHotel.GameClients;
    using Outgoing.Help;

    internal class SubmitBullyReportEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null)
            {
                return;
            }

            var userId = packet.PopInt();
            if (userId == session.GetHabbo().Id) //Hax
            {
                return;
            }

            if (session.GetHabbo().AdvertisingReportedBlocked)
            {
                session.SendPacket(new SubmitBullyReportComposer(1)); //This user is blocked from reporting.
                return;
            }

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(userId));
            if (client == null)
            {
                session.SendPacket(new SubmitBullyReportComposer(0)); //Just say it's sent, the user isn't found.
                return;
            }

            if (session.GetHabbo().LastAdvertiseReport > PlusEnvironment.GetUnixTimestamp())
            {
                session.SendNotification("Reports can only be sent per 5 minutes!");
                return;
            }

            if (client.GetHabbo().GetPermissions().HasRight("mod_tool")) //Reporting staff, nope!
            {
                session.SendNotification("Sorry, you cannot report staff members via this tool.");
                return;
            }

            //This user hasn't even said a word, nope!
            if (!client.GetHabbo().HasSpoken)
            {
                session.SendPacket(new SubmitBullyReportComposer(2));
                return;
            }

            //Already reported, nope.
            if (client.GetHabbo().AdvertisingReported && session.GetHabbo().Rank < 2)
            {
                session.SendPacket(new SubmitBullyReportComposer(3));
                return;
            }

            if (session.GetHabbo().Rank <= 1)
            {
                session.GetHabbo().LastAdvertiseReport = PlusEnvironment.GetUnixTimestamp() + 300;
            }
            else
            {
                session.GetHabbo().LastAdvertiseReport = PlusEnvironment.GetUnixTimestamp();
            }

            client.GetHabbo().AdvertisingReported = true;
            session.SendPacket(new SubmitBullyReportComposer(0));

            //PlusEnvironment.GetGame().GetClientManager().ModAlert("New advertising report! " + Client.GetHabbo().Username + " has been reported for advertising by " + Session.GetHabbo().Username +".");
            PlusEnvironment.GetGame().GetClientManager().DoAdvertisingReport(session, client);
        }
    }
}