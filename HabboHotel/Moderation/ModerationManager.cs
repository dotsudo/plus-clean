﻿namespace Plus.HabboHotel.Moderation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using log4net;

    public sealed class ModerationManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Plus.HabboHotel.Moderation.ModerationManager");
        private readonly Dictionary<string, ModerationBan> _bans = new Dictionary<string, ModerationBan>();

        private readonly Dictionary<int, List<ModerationPresetActions>> _moderationCfhTopicActions =
            new Dictionary<int, List<ModerationPresetActions>>();

        private readonly Dictionary<int, string> _moderationCfhTopics = new Dictionary<int, string>();

        private readonly ConcurrentDictionary<int, ModerationTicket> _modTickets =
            new ConcurrentDictionary<int, ModerationTicket>();

        private readonly List<string> _roomPresets = new List<string>();
        private readonly Dictionary<int, string> _userActionPresetCategories = new Dictionary<int, string>();

        private readonly Dictionary<int, List<ModerationPresetActionMessages>> _userActionPresetMessages =
            new Dictionary<int, List<ModerationPresetActionMessages>>();

        private readonly List<string> _userPresets = new List<string>();

        private int _ticketCount = 1;

        public ICollection<string> UserMessagePresets => _userPresets;

        public ICollection<string> RoomMessagePresets => _roomPresets;

        public ICollection<ModerationTicket> GetTickets => _modTickets.Values;

        public Dictionary<string, List<ModerationPresetActions>> UserActionPresets
        {
            get
            {
                var result = new Dictionary<string, List<ModerationPresetActions>>();
                foreach (var category in _moderationCfhTopics.ToList())
                {
                    result.Add(category.Value, new List<ModerationPresetActions>());
                    if (_moderationCfhTopicActions.ContainsKey(category.Key))
                    {
                        foreach (var data in _moderationCfhTopicActions[category.Key])
                        {
                            result[category.Value].Add(data);
                        }
                    }
                }

                return result;
            }
        }

        public void Init()
        {
            if (_userPresets.Count > 0)
            {
                _userPresets.Clear();
            }
            if (_moderationCfhTopics.Count > 0)
            {
                _moderationCfhTopics.Clear();
            }
            if (_moderationCfhTopicActions.Count > 0)
            {
                _moderationCfhTopicActions.Clear();
            }
            if (_bans.Count > 0)
            {
                _bans.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable presetsTable = null;
                dbClient.SetQuery("SELECT * FROM `moderation_presets`;");
                presetsTable = dbClient.GetTable();
                if (presetsTable != null)
                {
                    foreach (DataRow row in presetsTable.Rows)
                    {
                        var type = Convert.ToString(row["type"]).ToLower();
                        switch (type)
                        {
                            case "user":
                                _userPresets.Add(Convert.ToString(row["message"]));
                                break;
                            case "room":
                                _roomPresets.Add(Convert.ToString(row["message"]));
                                break;
                        }
                    }
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable moderationTopics = null;
                dbClient.SetQuery("SELECT * FROM `moderation_topics`;");
                moderationTopics = dbClient.GetTable();
                if (moderationTopics != null)
                {
                    foreach (DataRow row in moderationTopics.Rows)
                    {
                        if (!_moderationCfhTopics.ContainsKey(Convert.ToInt32(row["id"])))
                        {
                            _moderationCfhTopics.Add(Convert.ToInt32(row["id"]), Convert.ToString(row["caption"]));
                        }
                    }
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable moderationTopicsActions = null;
                dbClient.SetQuery("SELECT * FROM `moderation_topic_actions`;");
                moderationTopicsActions = dbClient.GetTable();
                if (moderationTopicsActions != null)
                {
                    foreach (DataRow row in moderationTopicsActions.Rows)
                    {
                        var parentId = Convert.ToInt32(row["parent_id"]);
                        if (!_moderationCfhTopicActions.ContainsKey(parentId))
                        {
                            _moderationCfhTopicActions.Add(parentId, new List<ModerationPresetActions>());
                        }
                        _moderationCfhTopicActions[parentId]
                            .Add(new ModerationPresetActions(Convert.ToInt32(row["id"]),
                                Convert.ToInt32(row["parent_id"]),
                                Convert.ToString(row["type"]),
                                Convert.ToString(row["caption"]),
                                Convert.ToString(row["message_text"]),
                                Convert.ToInt32(row["mute_time"]),
                                Convert.ToInt32(row["ban_time"]),
                                Convert.ToInt32(row["ip_time"]),
                                Convert.ToInt32(row["trade_lock_time"]),
                                Convert.ToString(row["default_sanction"])));
                    }
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable presetsActionCats = null;
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_categories`;");
                presetsActionCats = dbClient.GetTable();
                if (presetsActionCats != null)
                {
                    foreach (DataRow row in presetsActionCats.Rows)
                    {
                        _userActionPresetCategories.Add(Convert.ToInt32(row["id"]), Convert.ToString(row["caption"]));
                    }
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable presetsActionMessages = null;
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_messages`;");
                presetsActionMessages = dbClient.GetTable();
                if (presetsActionMessages != null)
                {
                    foreach (DataRow row in presetsActionMessages.Rows)
                    {
                        var parentId = Convert.ToInt32(row["parent_id"]);
                        if (!_userActionPresetMessages.ContainsKey(parentId))
                        {
                            _userActionPresetMessages.Add(parentId, new List<ModerationPresetActionMessages>());
                        }
                        _userActionPresetMessages[parentId]
                            .Add(new ModerationPresetActionMessages(Convert.ToInt32(row["id"]),
                                Convert.ToInt32(row["parent_id"]),
                                Convert.ToString(row["caption"]),
                                Convert.ToString(row["message_text"]),
                                Convert.ToInt32(row["mute_hours"]),
                                Convert.ToInt32(row["ban_hours"]),
                                Convert.ToInt32(row["ip_ban_hours"]),
                                Convert.ToInt32(row["trade_lock_days"]),
                                Convert.ToString(row["notice"])));
                    }
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable getBans = null;
                dbClient.SetQuery(
                    "SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                getBans = dbClient.GetTable();
                if (getBans != null)
                {
                    foreach (DataRow dRow in getBans.Rows)
                    {
                        var value = Convert.ToString(dRow["value"]);
                        var reason = Convert.ToString(dRow["reason"]);
                        var expires = (double) dRow["expire"];
                        var type = Convert.ToString(dRow["bantype"]);
                        var ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (ban != null)
                        {
                            if (expires > PlusEnvironment.GetUnixTimestamp())
                            {
                                if (!_bans.ContainsKey(value))
                                {
                                    _bans.Add(value, ban);
                                }
                            }
                            else
                            {
                                dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" +
                                                  BanTypeUtility.FromModerationBanType(ban.Type) +
                                                  "' AND `value` = @Key LIMIT 1");
                                dbClient.AddParameter("Key", value);
                                dbClient.RunQuery();
                            }
                        }
                    }
                }
            }

            Log.Info("Loaded " + (_userPresets.Count + _roomPresets.Count) + " moderation presets.");
            Log.Info("Loaded " + _userActionPresetCategories.Count + " moderation categories.");
            Log.Info("Loaded " + _userActionPresetMessages.Count + " moderation action preset messages.");
            Log.Info("Cached " + _bans.Count + " username and machine bans.");
        }

        public void ReCacheBans()
        {
            if (_bans.Count > 0)
            {
                _bans.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable getBans = null;
                dbClient.SetQuery(
                    "SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                getBans = dbClient.GetTable();
                if (getBans != null)
                {
                    foreach (DataRow dRow in getBans.Rows)
                    {
                        var value = Convert.ToString(dRow["value"]);
                        var reason = Convert.ToString(dRow["reason"]);
                        var expires = (double) dRow["expire"];
                        var type = Convert.ToString(dRow["bantype"]);
                        var ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (ban != null)
                        {
                            if (expires > PlusEnvironment.GetUnixTimestamp())
                            {
                                if (!_bans.ContainsKey(value))
                                {
                                    _bans.Add(value, ban);
                                }
                            }
                            else
                            {
                                dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" +
                                                  BanTypeUtility.FromModerationBanType(ban.Type) +
                                                  "' AND `value` = @Key LIMIT 1");
                                dbClient.AddParameter("Key", value);
                                dbClient.RunQuery();
                            }
                        }
                    }
                }
            }

            Log.Info("Cached " + _bans.Count + " username and machine bans.");
        }

        public void BanUser(string mod, ModerationBanType type, string banValue, string reason, double expireTimestamp)
        {
            var banType = type == ModerationBanType.Ip ? "ip" : type == ModerationBanType.Machine ? "machine" : "user";
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "REPLACE INTO `bans` (`bantype`, `value`, `reason`, `expire`, `added_by`,`added_date`) VALUES ('" +
                    banType +
                    "', '" +
                    banValue +
                    "', @reason, " +
                    expireTimestamp +
                    ", '" +
                    mod +
                    "', '" +
                    PlusEnvironment.GetUnixTimestamp() +
                    "');");
                dbClient.AddParameter("reason", reason);
                dbClient.RunQuery();
            }
            if (type == ModerationBanType.Machine || type == ModerationBanType.Username)
            {
                if (!_bans.ContainsKey(banValue))
                {
                    _bans.Add(banValue, new ModerationBan(type, banValue, reason, expireTimestamp));
                }
            }
        }

        public bool TryAddTicket(ModerationTicket ticket)
        {
            ticket.Id = _ticketCount++;
            return _modTickets.TryAdd(ticket.Id, ticket);
        }

        public bool TryGetTicket(int ticketId, out ModerationTicket ticket) => _modTickets.TryGetValue(ticketId, out ticket);

        public bool UserHasTickets(int userId)
        {
            return _modTickets.Count(x => x.Value.Sender.Id == userId && x.Value.Answered == false) > 0;
        }

        public ModerationTicket GetTicketBySenderId(int userId)
        {
            return _modTickets.FirstOrDefault(x => x.Value.Sender.Id == userId).Value;
        }

        public bool IsBanned(string key, out ModerationBan ban)
        {
            if (_bans.TryGetValue(key, out ban))
            {
                if (!ban.Expired)
                {
                    return true;
                }

                //This ban has expired, let us quickly remove it here.
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" +
                                      BanTypeUtility.FromModerationBanType(ban.Type) +
                                      "' AND `value` = @Key LIMIT 1");
                    dbClient.AddParameter("Key", key);
                    dbClient.RunQuery();
                }

                //And finally, let us remove the ban record from the cache.
                if (_bans.ContainsKey(key))
                {
                    _bans.Remove(key);
                }
                return false;
            }

            return false;
        }

        internal bool MachineBanCheck(string machineId)
        {
            if (!PlusEnvironment.GetGame().GetModerationManager().IsBanned(machineId, out _))
            {
                return true;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'machine' AND `value` = @value LIMIT 1");
                dbClient.AddParameter("value", machineId);

                var banRow = dbClient.GetRow();

                if (banRow != null)
                {
                    return true;
                }

                PlusEnvironment.GetGame().GetModerationManager().RemoveBan(machineId);
                return false;
            }
        }

        internal bool UsernameBanCheck(string username)
        {
            if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(username, out _))
            {
                DataRow banRow;
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'user' AND `value` = @value LIMIT 1");
                    dbClient.AddParameter("value", username);
                    banRow = dbClient.GetRow();

                    //If there is no more ban record, then we can simply remove it from our cache!
                    if (banRow == null)
                    {
                        PlusEnvironment.GetGame().GetModerationManager().RemoveBan(username);
                        return false;
                    }
                }
            }

            return true;
        }

        public void RemoveBan(string value)
        {
            _bans.Remove(value);
        }
    }
}