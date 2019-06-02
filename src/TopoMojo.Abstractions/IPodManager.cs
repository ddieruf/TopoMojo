// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

﻿using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.Abstractions
{
    public interface IPodManager : IHostedService
    {
        Task<Vm> Load(string id);
        Task<Vm> Start(string id);
        Task<Vm> Stop(string id);
        Task<Vm> Save(string id);
        Task<Vm> Revert(string id);
        Task<Vm> Delete(string id);
        Task<Vm> ChangeState(VmOperation op);
        Task<Vm> ChangeConfiguration(string id, KeyValuePair change);
        Task<Vm> Deploy(Template template);
        Task<Vm> Refresh(Template template);
        Task<Vm[]> Find(string searchText);
        Task<int> CreateDisks(Template template);
        Task<int> VerifyDisks(Template template);
        Task<int> DeleteDisks(Template template);
        Task<DisplayInfo> Display(string id);
        Task<Vm> Answer(string id, VmAnswer answer);
        Task<TemplateOptions> GetTemplateOptions(string key);
        Task<VmOptions> GetVmIsoOptions(string key);
        Task<VmOptions> GetVmNetOptions(string key);
        string Version { get; }
        Task ReloadHost(string host);

        PodConfiguration Options { get; }
    }

}
