namespace Plus.HabboHotel.Users.Process
{
    using System;
    using System.Threading;
    using Communication.Packets.Outgoing.Handshake;
    using log4net;

    internal sealed class ProcessComponent
    {
        private static readonly ILog Log = LogManager.GetLogger("Plus.HabboHotel.Users.Process.ProcessComponent");

        private static readonly int RuntimeInSec = 60;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);

        private bool _disabled;
        private Habbo _player;
        private Timer _timer;
        private bool _timerLagging;
        private bool _timerRunning;

        public bool Init(Habbo player)
        {
            if (player == null)
            {
                return false;
            }
            if (_player != null)
            {
                return false;
            }

            _player = player;
            _timer = new Timer(Run, null, RuntimeInSec * 1000, RuntimeInSec * 1000);
            return true;
        }

        public void Run(object state)
        {
            try
            {
                if (_disabled)
                {
                    return;
                }

                if (_timerRunning)
                {
                    _timerLagging = true;
                    Log.Warn("<Player " + _player.Id + "> Server can't keep up, Player timer is lagging behind.");
                    return;
                }

                _resetEvent.Reset();

                if (_player.TimeMuted > 0)
                {
                    _player.TimeMuted -= 60;
                }
                if (_player.MessengerSpamTime > 0)
                {
                    _player.MessengerSpamTime -= 60;
                }
                if (_player.MessengerSpamTime <= 0)
                {
                    _player.MessengerSpamCount = 0;
                }
                _player.TimeAfk += 1;
                if (_player.GetStats().RespectsTimestamp != DateTime.Today.ToString("MM/dd"))
                {
                    _player.GetStats().RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `user_stats` SET `dailyRespectPoints` = '" +
                                          (_player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20) +
                                          "', `dailyPetRespectPoints` = '" +
                                          (_player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20) +
                                          "', `respectsTimestamp` = '" +
                                          DateTime.Today.ToString("MM/dd") +
                                          "' WHERE `id` = '" +
                                          _player.Id +
                                          "' LIMIT 1");
                    }
                    _player.GetStats().DailyRespectPoints =
                        _player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20;
                    _player.GetStats().DailyPetRespectPoints =
                        _player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20;
                    if (_player.GetClient() != null)
                    {
                        _player.GetClient().SendPacket(new UserObjectComposer(_player));
                    }
                }
                if (_player.GiftPurchasingWarnings < 15)
                {
                    _player.GiftPurchasingWarnings = 0;
                }
                if (_player.MottoUpdateWarnings < 15)
                {
                    _player.MottoUpdateWarnings = 0;
                }
                if (_player.ClothingUpdateWarnings < 15)
                {
                    _player.ClothingUpdateWarnings = 0;
                }
                if (_player.GetClient() != null)
                {
                    PlusEnvironment.GetGame().GetAchievementManager()
                        .ProgressAchievement(_player.GetClient(), "ACH_AllTimeHotelPresence", 1);
                }
                _player.CheckCreditsTimer();
                _player.Effects().CheckEffectExpiry(_player);

                // END CODE

                // Reset the values
                _timerRunning = false;
                _timerLagging = false;
                _resetEvent.Set();
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            // Wait until any processing is complete first.
            try
            {
                _resetEvent.WaitOne(TimeSpan.FromMinutes(5));
            }
            catch
            {
            } // give up

            // Set the timer to disabled
            _disabled = true;

            // Dispose the timer to disable it.
            try
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
            catch
            {
            }

            // Remove reference to the timer.
            _timer = null;

            // Null the player so we don't reference it here anymore
            _player = null;
        }
    }
}