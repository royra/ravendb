﻿using System;
using System.Collections.Generic;

namespace Raven.WebConsole.ViewModels
{
    public class DatabasesViewModel
    {
        public IEnumerable<Database> Databases { get; set; }
        public string BaseUrl { get; set; }
        public bool More { get; set; }

        public class Database
        {
            public string Name { get; set; }
            public decimal SizeMb { get; set; }
            public IEnumerable<string> ActiveBundles { get; set; } 
            public DateTime? LastBackup { get; set; }

            public Database(string name, decimal sizeMb, IEnumerable<string> activeBundles, DateTime? lastBackup)
            {
                Name = name;
                SizeMb = sizeMb;
                ActiveBundles = activeBundles;
                LastBackup = lastBackup;
            }
        }
    }
}