using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;

namespace TopoMojo.vSphere
{
    public class PodManager : IPodManager
    {
        public PodManager(
            PodConfiguration options,
            //TemplateOptions templateOptions,
            ILoggerFactory mill
        )
        {
            _options = options;
            //_optTemplate = templateOptions;
            _mill = mill;
            _logger = _mill.CreateLogger<PodManager>();
            _hostCache = new Dictionary<string, VimHost>();
            _affinityMap = new Dictionary<string, VimHost>();
            _vmCache = new ConcurrentDictionary<string, Vm>();
            InitVlans();
            InitHost(_options.Url);
        }

        private readonly PodConfiguration _options;
        //private readonly TemplateOptions _optTemplate;
        private readonly ILogger<PodManager> _logger;
        private readonly ILoggerFactory _mill;

        private int _syncInterval = 20000;
        private Dictionary<string, VimHost> _hostCache;
        private Dictionary<string, int> _vlans;
        private BitArray _vlanMap;
        private TemplateOptions _cachedTemplateOptions;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private int _cacheExpirationThreshold = 3;
        private Dictionary<string, VimHost> _affinityMap;
        private ConcurrentDictionary<string, Vm> _vmCache;

        public PodConfiguration Options { get {return _options;}}
        public async Task ReloadHost(string hostname)
        {
            string host = "https://" + hostname + "/sdk";
            await AddHost(host);
        }

        //TODO: refactor this as InitializationProgress
        public async Task<Vm> Refresh(Template template)
        {
            string target = template.Name + "#" + template.IsolationTag;
            Vm vm = (await Find(target)).FirstOrDefault();
            if (vm == null)
            {
                vm = new Vm() { Name = target, Status = "created" };
                int progress = await VerifyDisks(template);
                if (progress == 100)
                    vm.Status = "initialized";
                else
                    if (progress >= 0)
                    {
                        vm.Task = new VmTask { Name = "initializing", Progress = progress };
                    }
            }

            //include task
            return vm;
        }
        public async Task<Vm> Deploy(Template template)
        {

            Vm[] vms = await Find(template.Name + "#" + template.IsolationTag);
            if (vms.Any())
                return vms.First();

            _logger.LogDebug("deploy: find host ");
            VimHost host = FindHostByAffinity(template.IsolationTag);
            _logger.LogDebug("deploy: host " + host.Name);
            NormalizeTemplate(template, host.Options);
            _logger.LogDebug("deploy: normalized "+ template.Name);

            if (!template.Disks.IsEmpty())
            {
                bool found = await host.FileExists(template.Disks[0].Path);
                if (!found)
                    throw new Exception("Template disks have not been prepared.");
            }

            _logger.LogDebug("deploy: reserve vlans ");
            ReserveVlans(template);

            _logger.LogDebug("deploy: " + template.Name + " " + host.Name);
            return await host.Deploy(template);
        }

        public async Task<Vm> Load(string id)
        {
            await Task.Delay(0);
            Vm vm = _vmCache.Values.Where(o=>o.Id == id).SingleOrDefault();
            CheckProgress(vm.Id);
            return vm;
        }

        private void CheckProgress(string id)
        {
            Vm vm = _vmCache.Values.Where(o=>o.Id == id).SingleOrDefault();
            if (vm.Task != null && (vm.Task.Progress < 0 || vm.Task.Progress > 99))
            {
                vm.Task = null;
                _vmCache.TryUpdate(vm.Id, vm, vm);
            }
        }

        private Vm[] CheckProgress(Vm[] vms)
        {
            foreach (Vm vm in vms)
                CheckProgress(vm.Id);
            return vms;
        }

        public async Task<Vm> Start(string id)
        {
            _logger.LogDebug("starting " + id);
            VimHost host = FindHostByVm(id);
            return await host.Start(id);
        }

        public async Task<Vm> Stop(string id)
        {
            _logger.LogDebug("stopping " + id);
            VimHost host = FindHostByVm(id);
            return await host.Stop(id);
        }

        public async Task<Vm> Save(string id)
        {
            _logger.LogDebug("saving " + id);
            VimHost host = FindHostByVm(id);
            return await host.Save(id);
        }

        public async Task<Vm> Revert(string id)
        {
            _logger.LogDebug("reverting " + id);
            VimHost host = FindHostByVm(id);
            return await host.Revert(id);
        }

        public async Task<Vm> Delete(string id)
        {
            _logger.LogDebug("deleting " + id);
            VimHost host = FindHostByVm(id);
            Vm vm =  await host.Delete(id);
            RefreshAffinity(); //TODO: fix race condition here
            return vm;
        }

        public async Task<Vm> Change(string id, KeyValuePair change)
        {
            _logger.LogDebug("changing " + id + " " + change.Key + "=" + change.Value);
            Vm vm = (await Find(id)).FirstOrDefault();
            if (vm == null)
                throw new InvalidOperationException();

            VimHost host = FindHostByVm(id);
            VmOptions vmo = null;
            //sanitze inputs
            if (change.Key == "iso")
            {
                vmo = await GetVmIsoOptions(vm.Name.Tag());
                if (!vmo.Iso.Contains(change.Value))
                    throw new InvalidOperationException();

                //translate display path back to actual path
                if (change.Value.StartsWith("public"))
                {
                    change.Value = host.Options.IsoStore + System.IO.Path.GetFileName(change.Value);
                }
                else
                {
                    change.Value = host.Options.DiskStore + vm.Name.Tag() + "/" + System.IO.Path.GetFileName(change.Value);
                }

            }

            if (change.Key == "net")
            {
                vmo = await GetVmNetOptions(vm.Name.Tag());
                if (!vmo.Net.Contains(change.Value))
                    throw new InvalidOperationException();
            }

            return await host.Change(id, change);
        }

        public async Task<Vm[]> Find(string term)
        {
            await Task.Delay(0);
            IEnumerable<Vm> q = _vmCache.Values;
            if (term.HasValue())
                q =  q.Where(o=>o.Id.Contains(term) || o.Name.Contains(term));
            return CheckProgress(q.ToArray());
        }

        public async Task<int> VerifyDisks(Template template)
        {
            foreach (VimHost vhost in _hostCache.Values)
            {
                int progress = await vhost.TaskProgress(template.Id);
                if (progress >= 0)
                    return progress;
            }

            string pattern = @"blank-(\d+)([^\.]+)";
            Match match = Regex.Match(template.Disks[0].Path, pattern);
            if (match.Success)
            {
                return 100; //show blank disk as created
            }

            VimHost host = FindHostByRandom();
            NormalizeTemplate(template, host.Options);
            if (template.Disks.Length > 0)
            {
                _logger.LogDebug(template.Source + " " + template.Disks[0].Path);
                if (await host.FileExists(template.Disks[0].Path))
                {
                    return 100;
                }
            }
            return -1;
        }

        public async Task<int> CreateDisks(Template template)
        {
            int progress = await VerifyDisks(template);
            if (progress < 0)
            {
                VimHost host = FindHostByRandom();
                Task cloneTask = host.CloneDisk(template.Id, template.Disks[0].Source, template.Disks[0].Path);
                progress = 0;
            }
            return progress;
        }

        public async Task<int> DeleteDisks(Template template)
        {
            int progress = await VerifyDisks(template);
            if (progress == 100)
            {
                VimHost host = FindHostByRandom();
                foreach (Disk disk in template.Disks)
                {
                    //protect stock disks; only delete a disk if it is local to the topology
                    //i.e. the disk folder matches the topologyId
                    if (template.IsolationTag.HasValue() && disk.Path.Contains(template.IsolationTag))
                    {
                        Task deleteTask = host.DeleteDisk(disk.Path);
                    }
                }
                return -1;
            }
            throw new Exception("Cannot delete disk that isn't fully created.");
        }

        public async Task<DisplayInfo> Display(string id)
        {
            DisplayInfo di = new DisplayInfo();
            Vm vm = Find(id).Result.FirstOrDefault();
            if (vm !=  null)
            {
                VimHost host = _hostCache[vm.Host];
                string ticket = "",
                    conditions = "";

                try
                {
                    ticket = await host.GetTicket(id);
                }
                catch //(System.Exception ex)
                {
                    conditions = "needs-vm-connected";
                }

                //string[] h = host.Name.Split('.');
                di = new DisplayInfo
                {
                    Id = id,
                    Name = vm.Name.Untagged(),
                    TopoId = vm.Name.Tag(),
                    Method = _options.DisplayMethod,
                    // Url = _options.DisplayUrl.Replace("{host}", targetHost) + ticket,
                    Url = ticket,
                    Conditions = conditions
                };
            }
            return di;
        }

        public async Task<Vm> Answer(string id, VmAnswer answer)
        {
            VimHost host = FindHostByVm(id);
            return await host.AnswerVmQuestion(id, answer.QuestionId, answer.ChoiceKey);
        }

        public async Task<TemplateOptions> GetTemplateOptionsFromCache(string key)
        {
            bool cacheExpired = DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes > _cacheExpirationThreshold
                || _cachedTemplateOptions == null;
            if (cacheExpired)
            {
                _cachedTemplateOptions = await GetTemplateOptions(key);
                _lastCacheUpdate = DateTime.Now;
            }
            return _cachedTemplateOptions;
        }

        public async Task<TemplateOptions> GetTemplateOptions(string key)
        {

            TemplateOptions to = new TemplateOptions();
            // {
            //     Cpu = _optTemplate.Cpu,
            //     Ram = _optTemplate.Ram,
            //     Adapters = _optTemplate.Adapters,
            //     Iso = (_optTemplate.Iso != null) ? _optTemplate.Iso : new string[] {},
            //     Source = (_optTemplate.Source != null) ? _optTemplate.Source : new string[] {},
            //     Guest = (_optTemplate.Guest != null) ? _optTemplate.Guest : new string[] {}
            // };

            // VimHost host = FindHostByRandom();
            // if (host != null)
            // {
            //         List<Task<string[]>> taskList = new List<Task<string[]>>();
            //         taskList.Add(host.GetFiles(host.Options.IsoStore + "public/*.iso", false));
            //         taskList.Add(host.GetFiles(host.Options.DiskStore + key + "/*.iso", false));
            //         taskList.Add(host.GetFiles(host.Options.StockStore + "*.vmdk", false));
            //         taskList.Add(host.GetGuestIds(""));
            //         var aggr = await Task.WhenAll<string[]>(taskList);
            //         to.Iso = to.Iso.Union(aggr[0].Union(aggr[1]))
            //             //.Select(x => x.Replace(".iso",""))
            //             .ToArray();
            //         to.Source = to.Source.Union(aggr[2])
            //             .Where(x=>!x.Contains("-flat"))
            //             //.Select(x => x.Replace(".vmdk", ""))
            //             .ToArray();
            //         to.Guest = to.Guest.Union(aggr[3]).ToArray();
            // }
            return await Task.FromResult(to);
        }

        public async Task<VmOptions> GetVmIsoOptions(string id)
        {
            VimHost host = FindHostByRandom();
            List<string> isos = new List<string>();

            //translate actual path to display path
            isos.AddRange(
                (await host.GetFiles(host.Options.IsoStore + "/*.iso", false))
                .Select(x => "public/" + System.IO.Path.GetFileName(x)).ToArray());
            isos.AddRange(
                (await host.GetFiles(host.Options.DiskStore + id + "/*.iso", false))
                .Select(x => System.IO.Path.GetFileName(x)));
            isos.Sort();
            return new VmOptions {
                Iso = isos.ToArray()
            };
        }

        public async Task<VmOptions> GetVmNetOptions(string id)
        {
            await Task.Delay(0);
            VimHost host = FindHostByRandom();
            List<string> nets = new List<string>();
            nets.AddRange(_vlans.Keys.Where(x => !x.Contains("#")));
            nets.AddRange(_vlans.Keys.Where(x => x.Contains(id)));
            nets.Sort();
            return new VmOptions {
                Net = nets.ToArray()
            };
        }
        public string Version
        {
            get
            {
                return "TopoMojo Pod Manager for vSphere, v1.0.0";
            }
        }

        private void NormalizeTemplate(Template template, PodConfiguration option)
        {
            if (!template.Iso.HasValue())
            {
                template.Iso = option.IsoStore + "null.iso";
            }
            else
            {
                template.Iso = (template.Iso.StartsWith("public"))
                    ? template.Iso = option.IsoStore + System.IO.Path.GetFileName(template.Iso)
                    : template.Iso = option.DiskStore + template.IsolationTag + "/" + System.IO.Path.GetFileName(template.Iso);
            }

            // if (template.Iso.HasValue() && !template.Iso.StartsWith(option.IsoStore))
            // {
            //     template.Iso = option.IsoStore + template.Iso + ".iso";
            // }

            if (template.Source.HasValue() && !template.Source.StartsWith(option.StockStore))
            {
                //template.Source = option.StockStore + template.Source + ".vmdk";
            }

            // string prefix = option.DiskStore.Trim();
            // if (!prefix.EndsWith("]") && !prefix.EndsWith("/"))
            //     prefix += "/";

            foreach (Disk disk in template.Disks)
            {
                if (!disk.Path.StartsWith(option.DiskStore)
                    && !disk.Path.StartsWith(option.StockStore))
                {
                    DatastorePath dspath = new DatastorePath(disk.Path);
                    dspath.Merge(option.DiskStore);
                    // string folder = dspath.FolderPath;
                    // if (folder.Trim().HasValue())
                    //     folder += "/";
                    // disk.Path = String.Format("{0}{1}/{2}", prefix, folder, dspath.File);
                    disk.Path = dspath.ToString();
                }
            }

            if (template.IsolationTag.HasValue())
            {
                string tag = "#" + template.IsolationTag;
                Regex rgx = new Regex("#.*");
                if (!template.Name.EndsWith(template.IsolationTag))
                    template.Name = rgx.Replace(template.Name, "") + tag;
                foreach (Eth eth in template.Eth)
                {
                    //don't add tag if referencing a global vlan
                    if (!_vlans.ContainsKey(eth.Net))
                    {
                        eth.Net = rgx.Replace(eth.Net, "") + tag;
                    }
                }
            }
        }

        private VimHost FindHostByVm(string id)
        {
            return _hostCache[_vmCache[id].Host];
        }

        private void RefreshAffinity()
        {
            lock(_affinityMap)
            {
                List<string> tags = new List<string>();
                foreach (Vm vm in _vmCache.Values)
                {
                    string tag = vm.Name.Tag();
                    tags.Add(tag);
                    if (!_affinityMap.ContainsKey(tag))
                        _affinityMap.Add(tag, _hostCache[vm.Host]);
                }
                string[] stale = _affinityMap.Keys.ToArray().Except(tags.Distinct().ToArray()).ToArray();
                foreach (string key in stale)
                    _affinityMap.Remove(key);
            }
        }

        private VimHost FindHostByAffinity(string tag)
        {
            VimHost host = null;
            lock(_affinityMap)
            {
                if (_affinityMap.ContainsKey(tag))
                    host =  _affinityMap[tag];
                else
                {
                    Vm vm = _vmCache.Values.Where(o=>o.Name.EndsWith(tag)).FirstOrDefault();
                    if (vm !=  null)
                        host = _hostCache[vm.Host];
                    else
                        host = FindHostByFewestVms();
                    _affinityMap.Add(tag, host);
                }
            }
            return host;
        }

        private VimHost FindHostByFewestVms()
        {
            Dictionary<string, HostVmCount> hostCounts = new Dictionary<string, HostVmCount>();
            foreach (VimHost host in _hostCache.Values)
            {
                if (!hostCounts.ContainsKey(host.Name))
                    hostCounts.Add(host.Name, new HostVmCount { Name = host.Name });
            }
            foreach (Vm vm in _vmCache.Values)
            {
                if (!hostCounts.ContainsKey(vm.Host))
                    hostCounts.Add(vm.Host, new HostVmCount { Name = vm.Host });
                hostCounts[vm.Host].Count += 1;
            }

            string hostname = hostCounts.Values
                .OrderBy(h => h.Count)
                .Select(h => h.Name)
                .FirstOrDefault();

            // string hostname = _vmCache.Values
            //     .GroupBy(o=>o.Host)
            //     .Select(g=> new { Host = g.Key, Count = g.Count()})
            //     .OrderBy(o=>o.Count).Select(o=>o.Host)
            //     .FirstOrDefault();

            if (hostname.HasValue() && _hostCache.ContainsKey(hostname))
                return _hostCache[hostname];
            else
                return FindHostByRandom();
        }

        private VimHost FindHostByRandom()
        {
            int i = new Random().Next(0, _hostCache.Values.Count() - 1);
            return _hostCache.Values.ElementAt(i);
        }

        private void InitHost(string host)
        {
            List<string> hosts = new List<string>();
            string match = new Regex(@"\[[\d-,]*\]").Match(host).Value;
            if (match.HasValue())
            {
                foreach(int i in match.ExpandRange())
                    hosts.Add(host.Replace(match, i.ToString()));
            }
            else
            {
                hosts.Add(host);
            }

            foreach (string url in hosts)
            {
                AddHost(url);
                //string hostname = new Uri(url).Host;
                // PodConfiguration hostOptions = (PodConfiguration)Helper.Clone(_options);
                // hostOptions.Url = url;
                // hostOptions.Host = hostname;
                // VimHost vHost = new VimHost(
                //     hostOptions,
                //     _vmCache,
                //     _vlanMap,
                //     _mill.CreateLogger<VimHost>()
                // );
                // _hostCache.Add(hostname, vHost);
            }
        }

        private async Task AddHost(string url)
        {
            string hostname = new Uri(url).Host;

            if (_hostCache.ContainsKey(hostname))
            {
                await _hostCache[hostname].Disconnect();
                _hostCache.Remove(hostname);
                await Task.Delay(100);
            }

            PodConfiguration hostOptions = (PodConfiguration)Helper.Clone(_options);
            hostOptions.Url = url;
            hostOptions.Host = hostname;
            VimHost vHost = new VimHost(
                hostOptions,
                _vmCache,
                _vlanMap,
                _mill.CreateLogger<VimHost>()
            );
            _hostCache.Add(hostname, vHost);
        }

        private void InitVlans()
        {
            //initialize vlan map
            _vlanMap = new BitArray(4096, true);
            foreach (int i in _options.Vlan.Range.ExpandRange())
            {
                _vlanMap[i] = false;
            }

            //set admin reservations
            _vlans = new Dictionary<string,int>();
            foreach (Vlan vlan in _options.Vlan.Reservations)
            {
                _vlans.Add(vlan.Name, vlan.Id);
                _vlanMap[vlan.Id] = true;
            }

            UpdateVlanReservationsLoop();
        }

        private void ReserveVlans(Template template)
        {
            lock (_vlanMap)
            {
                foreach (Eth eth in template.Eth)
                {
                    //if net already reserved, use reserved vlan
                    if (_vlans.ContainsKey(eth.Net))
                    {
                        eth.Vlan = _vlans[eth.Net];
                    }
                    else
                    {
                        int id = 0;
                        if (template.UseUplinkSwitch)
                        {
                            //get available uplink vlan
                            while (id < _vlanMap.Length && _vlanMap[id])
                            {
                                id += 1;
                            }

                            if (id > 0 && id < _vlanMap.Length)
                            {
                                eth.Vlan = id;
                                _vlanMap[id] = true;
                                _vlans.Add(eth.Net, id);
                            }
                            else
                            {
                                throw new Exception("Unable to reserve a vlan for " + eth.Net);
                            }
                        }
                        else {
                            //get highest vlan in this isolation group
                            id = 100;
                            foreach (string key in _vlans.Keys.Where(k => k.EndsWith(template.IsolationTag)))
                                id = Math.Max(id, _vlans[key]);
                            id += 1;
                            eth.Vlan = id;
                            _vlans.Add(eth.Net, id);
                        }

                    }
                }
            }
        }

        private async void UpdateVlanReservationsLoop()
        {
            while(true)
            {
                lock (_vlans)
                {
                    List<string> active = new List<string>();
                    foreach (VimHost host in _hostCache.Values)
                    {
                        foreach (Vlan vlan in host.PgCache)
                        {
                            //reconstitue vlan reservations from active vlans
                            if (!_vlans.ContainsKey(vlan.Name))
                                _vlans.Add(vlan.Name, vlan.Id);

                            active.Add(vlan.Name);
                            _vlanMap[vlan.Id] = true;
                        }
                    }

                    string[] reserved = _vlans.Keys.Where(x=>x.Contains("#")).ToArray();
                    string[] stale = reserved.Except(active).ToArray();

                    foreach(string net in stale)
                    {
                        if (_vlans.ContainsKey(net)
                            && !Find(net.Tag()).Result.Any() //don't remove if vm's are still deployed in this group
                        )
                        {
                            _logger.LogDebug("removing stale vlan reservation " + net);
                            _vlanMap[_vlans[net]] = false;
                            _vlans.Remove(net);
                        }
                    }
                }

                await Task.Delay(_syncInterval);
            }
        }

        protected class HostVmCount
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }
    }

}