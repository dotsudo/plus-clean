namespace Plus.Core
{
    using System;
    using Communication.Packets.Outgoing.Moderation;
    using log4net;

    public class ConsoleCommands
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.Core.ConsoleCommands");

        public static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData))
            {
                return;
            }

            try
            {
                var parameters = inputData.Split(' ');
                switch (parameters[0].ToLower())
                {
                    case "stop":
                    case "shutdown":
                    {
                        log.Warn(
                            "The server is saving users furniture, rooms, etc. WAIT FOR THE SERVER TO CLOSE, DO NOT EXIT THE PROCESS IN TASK MANAGER!!");
                        PlusEnvironment.PerformShutDown();
                        break;
                    }
                    case "alert":
                    {
                        var Notice = inputData.Substring(6);
                        PlusEnvironment.GetGame()
                            .GetClientManager()
                            .SendPacket(new BroadcastMessageAlertComposer(
                                PlusEnvironment.GetLanguageManager().TryGetValue("server.console.alert") +
                                "\n\n" +
                                Notice));
                        log.Info("Alert successfully sent.");
                        break;
                    }
                    default:
                    {
                        log.Error(parameters[0].ToLower() +
                                  " is an unknown or unsupported command. Type help for more information");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Error in command [" + inputData + "]: " + e);
            }
        }
    }
}