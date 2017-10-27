namespace Plus.HabboHotel.Permissions
{
    internal class PermissionCommand
    {
        public PermissionCommand(string Command, int GroupId, int SubscriptionId)
        {
            this.Command = Command;
            this.GroupId = GroupId;
            this.SubscriptionId = SubscriptionId;
        }

        public string Command { get; set; }
        public int GroupId { get; set; }
        public int SubscriptionId { get; set; }
    }
}