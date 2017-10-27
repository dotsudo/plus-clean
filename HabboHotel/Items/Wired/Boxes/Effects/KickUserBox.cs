namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System.Collections;
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Rooms;
    using Users;

    internal class KickUserBox : IWiredItem, IWiredCycle
    {
        private readonly Queue _toKick;

        public KickUserBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            _toKick = new Queue();
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }
        }

        public int TickCount { get; set; }
        public int Delay { get; set; }

        public bool OnCycle()
        {
            if (Instance == null)
            {
                return false;
            }

            if (_toKick.Count == 0)
            {
                TickCount = 3;
                return true;
            }

            lock (_toKick.SyncRoot)
            {
                while (_toKick.Count > 0)
                {
                    var player = (Habbo) _toKick.Dequeue();
                    if (player == null || !player.InRoom || player.CurrentRoom != Instance)
                    {
                        continue;
                    }

                    Instance.GetRoomUserManager().RemoveUserFromRoom(player.GetClient(), true);
                }
            }

            TickCount = 3;
            return true;
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectKickUser;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket packet)
        {
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }
            var unknown = packet.PopInt();
            var message = packet.PopString();
            StringData = message;
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length != 1)
            {
                return false;
            }

            var player = (Habbo) Params[0];
            if (player == null)
            {
                return false;
            }

            if (TickCount <= 0)
            {
                TickCount = 3;
            }
            if (!_toKick.Contains(player))
            {
                var user = Instance.GetRoomUserManager().GetRoomUserByHabbo(player.Id);
                if (user == null)
                {
                    return false;
                }

                if (player.GetPermissions().HasRight("mod_tool") || Instance.OwnerId == player.Id)
                {
                    player.GetClient()
                        .SendPacket(new WhisperComposer(user.VirtualId, "Wired Kick Exception: Unkickable Player", 0, 0));
                    return false;
                }

                _toKick.Enqueue(player);
                player.GetClient().SendPacket(new WhisperComposer(user.VirtualId, StringData, 0, 0));
            }

            return true;
        }
    }
}