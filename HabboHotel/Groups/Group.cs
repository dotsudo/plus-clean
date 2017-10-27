namespace Plus.HabboHotel.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public class Group
    {
        private readonly List<int> _administrators;
        private readonly List<int> _members;
        private readonly List<int> _requests;

        internal Group(int id,
                       string name,
                       string description,
                       string badge,
                       int roomId,
                       int owner,
                       int time,
                       int type,
                       int colour1,
                       int colour2,
                       int adminOnlyDeco)
        {
            Id = id;
            Name = name;
            Description = description;
            RoomId = roomId;
            Badge = badge;
            CreateTime = time;
            CreatorId = owner;
            Colour1 = colour1 == 0 ? 1 : colour1;
            Colour2 = colour2 == 0 ? 1 : colour2;

            switch (type)
            {
                case 0:
                    GroupType = GroupType.OPEN;
                    break;
                case 1:
                    GroupType = GroupType.LOCKED;
                    break;
                case 2:
                    GroupType = GroupType.PRIVATE;
                    break;
            }

            AdminOnlyDeco = adminOnlyDeco;
            ForumEnabled = ForumEnabled;
            _members = new List<int>();
            _requests = new List<int>();
            _administrators = new List<int>();
            InitMembers();
        }

        internal int Id { get; set; }
        internal string Name { get; set; }
        internal int AdminOnlyDeco { get; set; }
        internal string Badge { get; set; }
        internal int CreateTime { get; }
        internal int CreatorId { get; }
        internal string Description { get; set; }
        internal int RoomId { get; }
        internal int Colour1 { get; set; }
        internal int Colour2 { get; set; }
        internal bool ForumEnabled { get; }
        internal GroupType GroupType { get; set; }

        internal List<int> GetRequests => _requests.ToList();
        internal IEnumerable<int> GetAdministrators => _administrators.ToList();
        internal int MemberCount => _members.Count + _administrators.Count;
        internal int RequestCount => _requests.Count;

        internal IEnumerable<int> GetAllMembers
        {
            get
            {
                var members = new List<int>(_administrators.ToList());
                members.AddRange(_members.ToList());
                return members;
            }
        }

        private void InitMembers()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id`, `rank` FROM `group_memberships` WHERE `group_id` = @id");
                dbClient.AddParameter("id", Id);

                var getMembers = dbClient.GetTable();

                if (getMembers != null)

                {
                    foreach (DataRow row in getMembers.Rows)
                    {
                        var userId = Convert.ToInt32(row["user_id"]);
                        var isAdmin = Convert.ToInt32(row["rank"]) != 0;
                        if (isAdmin)
                        {
                            if (!_administrators.Contains(userId))
                            {
                                _administrators.Add(userId);
                            }
                        }
                        else
                        {
                            if (!_members.Contains(userId))
                            {
                                _members.Add(userId);
                            }
                        }
                    }
                }

                dbClient.SetQuery("SELECT `user_id` FROM `group_requests` WHERE `group_id` = @id");
                dbClient.AddParameter("id", Id);
                var getRequests = dbClient.GetTable();

                if (getRequests == null)
                {
                    return;
                }

                foreach (DataRow row in getRequests.Rows)
                {
                    var userId = Convert.ToInt32(row["user_id"]);
                    if (_members.Contains(userId) || _administrators.Contains(userId))
                    {
                        dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Id + "' AND `user_id` = '" +
                                          userId + "'");
                    }
                    else if (!_requests.Contains(userId))
                    {
                        _requests.Add(userId);
                    }
                }
            }
        }

        internal bool IsMember(int id) => _members.Contains(id) || _administrators.Contains(id);
        internal bool IsAdmin(int id) => _administrators.Contains(id);
        internal bool HasRequest(int id) => _requests.Contains(id);

        internal void MakeAdmin(int id)
        {
            if (_members.Contains(id))
            {
                _members.Remove(id);
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE group_memberships SET `rank` = '1' WHERE `user_id` = @uid AND `group_id` = @gid LIMIT 1");
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", id);
                dbClient.RunQuery();
            }
            if (!_administrators.Contains(id))
            {
                _administrators.Add(id);
            }
        }

        internal void TakeAdmin(int userId)
        {
            if (!_administrators.Contains(userId))
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE group_memberships SET `rank` = '0' WHERE user_id = @uid AND group_id = @gid");
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", userId);
                dbClient.RunQuery();
            }
            _administrators.Remove(userId);
            _members.Add(userId);
        }

        internal void AddMember(int id)
        {
            if (IsMember(id) || GroupType == GroupType.LOCKED && _requests.Contains(id))
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (IsAdmin(id))
                {
                    dbClient.SetQuery("UPDATE `group_memberships` SET `rank` = '0' WHERE user_id = @uid AND group_id = @gid");
                    _administrators.Remove(id);
                    _members.Add(id);
                }
                else if (GroupType == GroupType.LOCKED)
                {
                    dbClient.SetQuery("INSERT INTO `group_requests` (user_id, group_id) VALUES (@uid, @gid)");
                    _requests.Add(id);
                }
                else
                {
                    dbClient.SetQuery("INSERT INTO `group_memberships` (user_id, group_id) VALUES (@uid, @gid)");
                    _members.Add(id);
                }

                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", id);
                dbClient.RunQuery();
            }
        }

        internal void DeleteMember(int id)
        {
            if (IsMember(id))
            {
                if (_members.Contains(id))
                {
                    _members.Remove(id);
                }
            }
            else if (IsAdmin(id))
            {
                if (_administrators.Contains(id))
                {
                    _administrators.Remove(id);
                }
            }
            else
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM group_memberships WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", id);
                dbClient.RunQuery();
            }
        }

        internal void HandleRequest(int id, bool accepted)
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (accepted)
                {
                    dbClient.SetQuery("INSERT INTO group_memberships (user_id, group_id) VALUES (@uid, @gid)");
                    dbClient.AddParameter("gid", Id);
                    dbClient.AddParameter("uid", id);
                    dbClient.RunQuery();
                    _members.Add(id);
                }
                dbClient.SetQuery("DELETE FROM group_requests WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", id);
                dbClient.RunQuery();
            }

            if (_requests.Contains(id))
            {
                _requests.Remove(id);
            }
        }

        internal void ClearRequests()
        {
            _requests.Clear();
        }

        internal void Dispose()
        {
            _requests.Clear();
            _members.Clear();
            _administrators.Clear();
        }
    }
}