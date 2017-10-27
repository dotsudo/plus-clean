namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Users.Clothing.Parts;

    internal class FigureSetIdsComposer : ServerPacket
    {
        public FigureSetIdsComposer(ICollection<ClothingParts> clothingParts)
            : base(ServerPacketHeader.FigureSetIdsMessageComposer)
        {
            WriteInteger(clothingParts.Count);
            foreach (var part in clothingParts.ToList())
            {
                WriteInteger(part.PartId);
            }

            WriteInteger(clothingParts.Count);
            foreach (var part in clothingParts.ToList())
            {
                WriteString(part.Part);
            }
        }
    }
}