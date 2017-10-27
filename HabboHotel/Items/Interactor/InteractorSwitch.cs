namespace Plus.HabboHotel.Items.Interactor
{
    using GameClients;
    using Quests;
    using Rooms;

    internal class InteractorSwitch : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (session == null)
            {
                return;
            }

            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
            {
                var modes = item.GetBaseItem().Modes - 1;
                if (modes <= 0)
                {
                    return;
                }

                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniSwitch);
                var currentMode = 0;
                var newMode = 0;
                if (!int.TryParse(item.ExtraData, out currentMode))
                {
                }
                if (currentMode <= 0)
                {
                    newMode = 1;
                }
                else if (currentMode >= modes)
                {
                    newMode = 0;
                }
                else
                {
                    newMode = currentMode + 1;
                }
                item.ExtraData = newMode.ToString();
                item.UpdateState();
            }
            else
            {
                user.MoveTo(item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}