namespace Plus.HabboHotel.Cache.Process
{
    using System;
    using System.Linq;
    using System.Threading;
    using Core;
    using log4net;
    using Type;
    using Users;

    internal sealed class ProcessComponent
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Cache.Process.ProcessComponent");

        private static readonly int _runtimeInSec = 1200;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);

        private bool _disabled;

        private Timer _timer;

        private bool _timerLagging;

        private bool _timerRunning;

        public void Init()
        {
            _timer = new Timer(Run, null, _runtimeInSec * 1000, _runtimeInSec * 1000);
        }

        public void Run(object State)
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
                    return;
                }

                _resetEvent.Reset();

                // BEGIN CODE
                var CacheList = PlusEnvironment.GetGame().GetCacheManager().GetUserCache().ToList();
                if (CacheList.Count > 0)
                {
                    foreach (var Cache in CacheList)
                    {
                        try
                        {
                            if (Cache == null)
                            {
                                continue;
                            }

                            UserCache Temp = null;
                            if (Cache.isExpired())
                            {
                                PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(Cache.Id, out Temp);
                            }
                            Temp = null;
                        }
                        catch (Exception e)
                        {
                            ExceptionLogger.LogException(e);
                        }
                    }
                }

                CacheList = null;
                var CachedUsers = PlusEnvironment.GetUsersCached().ToList();
                if (CachedUsers.Count > 0)
                {
                    foreach (var Data in CachedUsers)
                    {
                        try
                        {
                            if (Data == null)
                            {
                                continue;
                            }

                            Habbo Temp = null;
                            if (Data.CacheExpired())
                            {
                                PlusEnvironment.RemoveFromCache(Data.Id, out Temp);
                            }
                            if (Temp != null)
                            {
                                Temp.Dispose();
                            }
                            Temp = null;
                        }
                        catch (Exception e)
                        {
                            ExceptionLogger.LogException(e);
                        }
                    }
                }

                CachedUsers = null;

                // END CODE

                // Reset the values
                _timerRunning = false;
                _timerLagging = false;
                _resetEvent.Set();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
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
        }
    }
}