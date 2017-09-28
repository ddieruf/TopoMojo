using System;
using System.Collections.Generic;
using System.Linq;
using TopoMojo.Extensions;
//using TopoMojo.Core;
//using TopoMojo.Data.Entities;
//using TopoMojo.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.Core.Models.Extensions
{
    public static class ModelExtensions
    {
        public static void MergeVms(this GameState state, Vm[] vms)
        {
            foreach (Vm vm in vms)
            {
                string name = vm.Name.Untagged();
                VmState vs = state.Vms
                    .Where(t => t.Name == name && !t.Id.HasValue())
                    .FirstOrDefault();

                if (vs != null)
                {
                    vs.Id = vm.Id;
                    vs.IsRunning = vm.State == VmPowerState.running;
                }
            }
        }
    }
}