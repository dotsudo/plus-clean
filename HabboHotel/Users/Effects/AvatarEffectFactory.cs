﻿namespace Plus.HabboHotel.Users.Effects
{
    using System;

    internal static class AvatarEffectFactory
    {
        public static AvatarEffect CreateNullable(Habbo habbo, int spriteId, double duration)
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `user_effects` (`user_id`,`effect_id`,`total_duration`,`is_activated`,`activated_stamp`,`quantity`) VALUES(@uid,@sid,@dur,'0',0,1)");
                dbClient.AddParameter("uid", habbo.Id);
                dbClient.AddParameter("sid", spriteId);
                dbClient.AddParameter("dur", duration);
                return new AvatarEffect(Convert.ToInt32(dbClient.InsertQuery()), habbo.Id, spriteId, duration, false, 0, 1);
            }
        }
    }
}