namespace Plus.HabboHotel.Moderation
{
    internal class ModerationPresetActionCategories
    {
        public ModerationPresetActionCategories(int id, string caption)
        {
            Id = id;
            Caption = caption;
        }

        public int Id { get; set; }
        public string Caption { get; set; }
    }
}