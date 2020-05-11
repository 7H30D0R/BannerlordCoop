﻿using System;
using System.Collections.Generic;
using System.Net;
using Coop.Network;
using TaleWorlds.Library;

namespace Coop.Game.CLI
{
    public static class CLICommands
    {
        private const string sGroupName = "coop";

        [CommandLineFunctionality.CommandLineArgumentFunction("info", sGroupName)]
        public static string DumpInfo(List<string> parameters)
        {
            string sMessage = "";
            sMessage += CoopServer.Instance + Environment.NewLine;
            sMessage += Environment.NewLine + "*** Client ***" + Environment.NewLine;
            sMessage += CoopClient.Instance + Environment.NewLine;
            return sMessage;
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("start_local_server", sGroupName)]
        public static string StartServer(List<string> parameters)
        {
            CoopServer.Instance.StartServer();
            ServerConfiguration config = CoopServer.Instance.Current.ActiveConfig;
            CoopClient.Instance.Connect(config.LanAddress, config.LanPort);
            return CoopServer.Instance.ToString();
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("connect_to", sGroupName)]
        public static string ConnectTo(List<string> parameters)
        {
            if (parameters.Count != 2 ||
                !IPAddress.TryParse(parameters[0], out IPAddress ip) ||
                !int.TryParse(parameters[1], out int iPort))
            {
                return $"Usage: \"{sGroupName}.connect_to [IP] [Port]\"." +
                       Environment.NewLine +
                       $"\tExample: \"{sGroupName}.connect_to 127.0.0.1 4201\".";
            }

            CoopClient.Instance.Connect(ip, iPort);
            return "Client connection request sent.";
        }
    }
}