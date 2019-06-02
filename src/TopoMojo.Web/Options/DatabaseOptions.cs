// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo
{
    public class DatabaseOptions
    {
        public bool IsDevelopment { get; set; }
        public bool AutoMigrate { get; set; }
        public bool DevModeRecreate { get; set; }
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string MigrationsAssembly { get; set; }
        public string SeedTemplateKey { get; set; } = "seed-data.json";
    }
}