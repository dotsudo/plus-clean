﻿namespace Plus.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ConfigurationData
    {
        internal readonly Dictionary<string, string> Data;

        internal ConfigurationData(string filePath, bool maynotexist = false)
        {
            Data = new Dictionary<string, string>();
            if (!File.Exists(filePath))
            {
                if (!maynotexist)
                {
                    throw new ArgumentException("Unable to locate configuration file at '" + filePath + "'.");
                }

                return;
            }

            try
            {
                using (var stream = new StreamReader(filePath))
                {
                    string  line;
                    
                    while ((line = stream.ReadLine()) != null)
                    {
                        if (line.Length < 1 || line.StartsWith("#"))
                        {
                            continue;
                        }

                        var delimiterIndex = line.IndexOf('=');

                        if (delimiterIndex == -1)
                        {
                            continue;
                        }

                        var key = line.Substring(0, delimiterIndex);
                        var val = line.Substring(delimiterIndex + 1);
                        Data.Add(key, val);
                    }
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not process configuration file: " + e.Message);
            }
        }
    }
}