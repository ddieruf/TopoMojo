using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Web;

namespace TopoMojo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class VmController : _Controller
    {
        public VmController(
            TemplateManager templateManager,
            TopologyManager topoManager,
            IPodManager podManager,
            IServiceProvider sp)
        :base(sp)
        {
            _mgr = templateManager;
            _topoManager = topoManager;
            _pod = podManager;
        }

        private readonly IPodManager _pod;
        private readonly TemplateManager _mgr;
        private readonly TopologyManager _topoManager;

        [HttpGet("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Refresh(int id)
        {
            TopoMojo.Models.Template template  = await _mgr.GetDeployableTemplate(id, null);

            Vm vm = await _pod.Refresh(template);
            return Json(vm);
        }
        [HttpGet("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Load(string id)
        {
            await AuthorizeAction(id, "load");
            Vm vm = await _pod.Load(id);
            return Json(vm);
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Start(string id)
        {
            await AuthorizeAction(id, "start");

            Vm vm = await _pod.Start(id);
            return Json(vm);
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Stop(string id)
        {
            await AuthorizeAction(id, "stop");
            Vm vm = await _pod.Stop(id);
            return Json(vm);
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Save(string id)
        {
            await AuthorizeAction(id, "save");
            Vm vm = await _pod.Save(id);
            return Json(vm);
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Revert(string id)
        {
            await AuthorizeAction(id, "revert");
            Vm vm = await _pod.Revert(id);
            return Json(vm);
        }

        [HttpDelete("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Delete([FromRoute]string id)
        {
            await AuthorizeAction(id, "delete");
            Vm vm = await _pod.Delete(id);
            return Json(vm);
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Change(string id, [FromBody] KeyValuePair change)
        {
            await AuthorizeAction(id, "change");

            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            return Json(await _pod.Change(id, change));
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Deploy(int id)
        {
            TopoMojo.Models.Template template  = await _mgr.GetDeployableTemplate(id, null);

            return Json(await _pod.Deploy(template));
        }

        [HttpPost("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Initialize(int id)
        {
            TopoMojo.Models.Template template  = await _mgr.GetDeployableTemplate(id, null);
            return Json(await _pod.CreateDisks(template));
        }

        [HttpGet("/[controller]/[action]/{id}")]
        public async Task<IActionResult> Display([FromRoute]string id)
        {
            await AuthorizeAction(id, "display");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();

            DisplayInfo info = await _pod.Display(id);
            if (info == null)
                return View("Error");

            info.TopoId = vm.Name.Tag();
            return View("wmks", info);
        }

        [HttpGet("/[controller]/[action]/{id}")]
        public async Task<IActionResult> Console([FromRoute]string id)
        {
            await AuthorizeAction(id, "console");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();

            DisplayInfo info = await _pod.Display(id);
            if (info == null)
                return View("Error");

            info.TopoId = vm.Name.Tag();
            return View("wmks", info);
        }

        [HttpPost("{id}/{question}/{answer}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> Answer(string id, string question, string answer)
        {
            await AuthorizeAction(id, "answer");
            return Json(await _pod.Answer(id, question, answer));
        }

        [HttpGet("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> IsoOptions(string id)
        {
            await AuthorizeAction(id, "isooptions");
            Vm vm = (await _pod.Find(id)).FirstOrDefault();

            //TODO: lookup TopoId from IsolationTag (for now they are the same)
            string tag = vm.Name.Tag();
            return Json(await _pod.GetVmIsoOptions(tag));
        }

        [HttpGet("{id}")]
        [JsonExceptionFilter]
        public async Task<IActionResult> NetOptions(string id)
        {
            await AuthorizeAction(id, "netoptions");

            Vm vm = (await _pod.Find(id)).FirstOrDefault();
            if (vm == null)
                return BadRequest();

            //TODO: lookup TopoId from IsolationTag (for now they are the same)
            string tag = vm.Name.Tag();
            return Json(await _pod.GetVmNetOptions(tag));
        }

        private async Task<bool> AuthorizeAction(string id, string method)
        {
            // if (_user.IsAdmin)
            //     return true;

            Vm vm = _pod.Find(id).Result.FirstOrDefault();
            string instanceId = vm.Name.Tag();
            if (String.IsNullOrEmpty(instanceId))
                throw new InvalidOperationException();

            bool result = await _topoManager.CanEdit(instanceId);

            if (!result && "display console load".Contains(method))
                result = await _topoManager.AllowedInstanceAccess(instanceId);

            if (!result)
                throw new InvalidOperationException();

            if (method != "load")
            {
                _logger.LogInformation($"vm-action {method} {id}");
            }
            return result;
        }

        // private bool AuthorizedForVm(Vm vm)
        // {
        //     return vm != null && AuthorizedForRoom(vm.Name.Tag());
        // }
    }


}
