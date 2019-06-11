// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TopoMojo.Services
{
    public class ServiceHostWrapper<T> : IHostedService
        where T : IHostedService
    {
        private readonly T backgroundService;

        public ServiceHostWrapper(T backgroundService)
        {
            this.backgroundService = backgroundService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return backgroundService.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return backgroundService.StopAsync(cancellationToken);
        }
    }
}