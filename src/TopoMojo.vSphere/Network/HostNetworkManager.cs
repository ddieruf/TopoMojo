using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TopoMojo.Models.Virtual;
using TopoMojo.vSphere.Helpers;

namespace TopoMojo.vSphere.Network
{
    public class HostNetworkManager : NetworkManager
    {
        public HostNetworkManager(
            Settings settings,
            ConcurrentDictionary<string, Vm> vmCache,
            VlanManager vlanManager
        ) : base(settings, vmCache, vlanManager)
        {

        }

        public override async Task<PortGroupAllocation> AddPortGroup(string sw, Eth eth)
        {
            try
            {
                HostPortGroupSpec spec = new HostPortGroupSpec();
                spec.vswitchName = sw;
                spec.vlanId = eth.Vlan;
                spec.name = eth.Net;
                spec.policy = new HostNetworkPolicy();
                spec.policy.security = new HostNetworkSecurityPolicy();
                spec.policy.security.allowPromiscuous = true;
                spec.policy.security.allowPromiscuousSpecified = true;

                await _client.vim.AddPortGroupAsync(_client.net, spec);

            } catch {}

            return new PortGroupAllocation
            {
                Net = eth.Net,
                Key = "HostPortGroup|"+eth.Net,
                VlanId = eth.Vlan,
                Switch = sw
            };
        }

        public override async Task AddSwitch(string sw)
        {
            HostVirtualSwitchSpec swspec = new HostVirtualSwitchSpec();
            swspec.numPorts = 32;
            // swspec.policy = new HostNetworkPolicy();
            // swspec.policy.security = new HostNetworkSecurityPolicy();
            await _client.vim.AddVirtualSwitchAsync(_client.net, sw, swspec);
        }

        public override async Task<VmNetwork[]> GetVmNetworks(ManagedObjectReference mor)
        {
            List<VmNetwork> result = new List<VmNetwork>();
            RetrievePropertiesResponse response = await _client.vim.RetrievePropertiesAsync(
                _client.props,
                FilterFactory.VmFilter(mor, "name config"));
            ObjectContent[] oc = response.returnval;
            foreach (ObjectContent obj in oc)
            {
                // if (!obj.IsInPool(_client.pool))
                //     continue;

                string vmName = obj.GetProperty("name").ToString();
                VirtualMachineConfigInfo config = obj.GetProperty("config") as VirtualMachineConfigInfo;
                foreach (VirtualEthernetCard card in config.hardware.device.OfType<VirtualEthernetCard>())
                {
                    result.Add(new VmNetwork
                    {
                        NetworkMOR = ((VirtualEthernetCardNetworkBackingInfo)card.backing).deviceName,
                        VmName = vmName
                    });
                }
            }
            return result.ToArray();
        }

        public override async Task<PortGroupAllocation[]> LoadPortGroups()
        {
            RetrievePropertiesResponse response = await _client.vim.RetrievePropertiesAsync(
                _client.props,
                FilterFactory.NetworkFilter(_client.net));

            ObjectContent[] oc = response.returnval;
            HostPortGroup[] pgs = (HostPortGroup[])oc[0].propSet[0].val;

            var list = new List<PortGroupAllocation>();

            foreach(HostPortGroup pg in pgs)
            {
                if (pg.spec.name.Contains("#"))
                    list.Add(new PortGroupAllocation
                    {
                        Net = pg.spec.name,
                        Key = "HostPortGroup|" + pg.spec.name,
                        VlanId = pg.spec.vlanId,
                        Switch = pg.spec.vswitchName
                    });
            }
            return list.ToArray();
        }

        public override async Task RemovePortgroup(string pgReference)
        {
            await _client.vim.RemovePortGroupAsync(_client.net, pgReference.AsReference().Value);
        }

        public override async Task RemoveSwitch(string sw)
        {
            await _client.vim.RemoveVirtualSwitchAsync(_client.net, sw);
        }
    }
}