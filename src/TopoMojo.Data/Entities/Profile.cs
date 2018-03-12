using System;
using System.Collections.Generic;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.Entities
{
    public class Profile: IEntityPrimary
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
        public bool IsAdmin { get; set; }
        public int WorkspaceLimit { get; set; }
        public virtual ICollection<Worker> Workspaces { get; set; } = new List<Worker>();
        public virtual ICollection<Player> Gamespaces { get; set; } = new List<Player>();
    }
}