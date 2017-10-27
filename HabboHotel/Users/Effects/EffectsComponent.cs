namespace Plus.HabboHotel.Users.Effects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Communication.Packets.Outgoing.Rooms.Avatar;

    public sealed class EffectsComponent
    {
        private readonly ConcurrentDictionary<int, AvatarEffect> _effects = new ConcurrentDictionary<int, AvatarEffect>();
        private Habbo _habbo;

        internal ICollection<AvatarEffect> GetAllEffects => _effects.Values;

        internal int CurrentEffect { get; set; }

        internal bool Init(Habbo habbo)
        {
            if (_effects.Count > 0)
            {
                return false;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_effects` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", habbo.Id);
                var getEffects = dbClient.GetTable();
                if (getEffects != null)
                {
                    foreach (DataRow row in getEffects.Rows)
                    {
                        if (_effects.TryAdd(Convert.ToInt32(row["id"]),
                            new AvatarEffect(Convert.ToInt32(row["id"]),
                                Convert.ToInt32(row["user_id"]),
                                Convert.ToInt32(row["effect_id"]),
                                Convert.ToDouble(row["total_duration"]),
                                PlusEnvironment.EnumToBool(row["is_activated"].ToString()),
                                Convert.ToDouble(row["activated_stamp"]),
                                Convert.ToInt32(row["quantity"]))))
                        {
                            //umm?
                        }
                    }
                }
            }

            _habbo = habbo;
            CurrentEffect = 0;
            return true;
        }

        internal bool HasEffect(int spriteId, bool activatedOnly = false, bool unactivatedOnly = false) =>
            GetEffectNullable(spriteId, activatedOnly, unactivatedOnly) != null;

        internal AvatarEffect GetEffectNullable(int spriteId, bool activatedOnly = false, bool unactivatedOnly = false)
        {
            return _effects.Values.ToList().FirstOrDefault(effect =>
                !effect.HasExpired && effect.SpriteId == spriteId && (!activatedOnly || effect.Activated) && (!unactivatedOnly || !effect.Activated));
        }

        internal void CheckEffectExpiry(Habbo habbo)
        {
            foreach (var effect in _effects.Values.ToList())
            {
                if (effect.HasExpired)
                {
                    effect.HandleExpiration(habbo);
                }
            }
        }

        internal void ApplyEffect(int effectId)
        {
            var user = _habbo?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(_habbo.Id);

            if (user == null)
            {
                return;
            }

            CurrentEffect = effectId;
            if (user.IsDancing)
            {
                _habbo.CurrentRoom.SendPacket(new DanceComposer(user, 0));
            }
            _habbo.CurrentRoom.SendPacket(new AvatarEffectComposer(user.VirtualId, effectId));
        }

        internal void Dispose()
        {
            _effects.Clear();
        }
    }
}