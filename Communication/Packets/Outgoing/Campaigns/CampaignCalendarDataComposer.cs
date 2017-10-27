namespace Plus.Communication.Packets.Outgoing.Campaigns
{
    using System;
    using System.Collections.Generic;

    internal class CampaignCalendarDataComposer : ServerPacket
    {
        public CampaignCalendarDataComposer(List<int> openedBoxes, List<int> lateBoxes)
            : base(ServerPacketHeader.CampaignCalendarDataMessageComposer)
        {
            WriteString("xmas15"); //Set the campaign.
            WriteString(""); //No idea.
            WriteInteger(DateTime.Now.Day - 1); //Start
            WriteInteger(25); //End?

            //Opened boxes
            WriteInteger(openedBoxes.Count);
            foreach (var day in openedBoxes)
            {
                WriteInteger(day);
            }

            //Late boxes?
            WriteInteger(lateBoxes.Count);
            foreach (var day in lateBoxes)
            {
                WriteInteger(day);
            }
        }
    }
}