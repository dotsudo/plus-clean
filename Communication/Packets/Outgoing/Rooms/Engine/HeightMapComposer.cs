namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using System;

    internal class HeightMapComposer : ServerPacket
    {
        public HeightMapComposer(string map)
            : base(ServerPacketHeader.HeightMapMessageComposer)
        {
            map = map.Replace("\n", "");
            var split = map.Split('\r');
            WriteInteger(split[0].Length);
            WriteInteger((split.Length - 1) * split[0].Length);
            
            int x;
            int y;
            
            for (y = 0; y < split.Length - 1; y++)
            {
                for (x = 0; x < split[0].Length; x++)
                {
                    char pos;

                    try
                    {
                        pos = split[y][x];
                    }
                    catch
                    {
                        pos = 'x';
                    }

                    if (pos == 'x')
                    {
                        WriteShort(-1);
                    }
                    else
                    {
                        if (int.TryParse(pos.ToString(), out var height))
                        {
                            height = height * 256;
                        }
                        else
                        {
                            height = (Convert.ToInt32(pos) - 87) * 256;
                        }
                        WriteShort(height);
                    }
                }
            }
        }
    }
}