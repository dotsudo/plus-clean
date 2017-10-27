namespace Plus.HabboHotel.Moderation
{
    internal class ModerationPreset
    {
        public ModerationPreset(int id, string type, string message)
        {
            Id = id;
            Type = type;
            Message = message;
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}