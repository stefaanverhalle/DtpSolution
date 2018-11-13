﻿using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpCore.Repository;
using DtpCore.ViewModel;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Trusts
{
    public class AddPackageCommandHandler : IRequestHandler<AddPackageCommand, NotificationSegment>
    {

        private IMediator _mediator;
        private TrustDBContext _db;
        private NotificationSegment _notifications;
        private readonly ILogger<AddPackageCommandHandler> _logger;

        public AddPackageCommandHandler(IMediator mediator, TrustDBContext db, NotificationSegment notifications, ILogger<AddPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<NotificationSegment> Handle(AddPackageCommand request, CancellationToken cancellationToken)
        {
            var package = request.Package;
            var trusts = package.Trusts;

            // Check for existing packages
            if ((package.Id != null && package.Id.Length > 0))
            {
                if (_db.Packages.Any(f => f.Id == package.Id))
                {
                    _notifications.Add(new PackageExistNotification { Package = package });
                    return _notifications;
                }

                package.Trusts = null; // Do not update the chilren yet. Special cases for them.
                _db.Packages.Add(package); // Get a DatabaseID for the package.
                _db.SaveChanges();
            }

            var packageResult = new PackageAddedResult();

            foreach (var trust in trusts)
            {
                if (package.DatabaseID > 0) // Create the relation between the package and trust
                    trust.TrustPackages.Add(new TrustPackage { PackageID = package.DatabaseID });

                var trustResult = await _mediator.Send(new AddTrustCommand { Trust = trust });

                if(package.DatabaseID > 0 && trustResult.LastOrDefault() is TrustObsoleteNotification)
                {
                    // Make sure to update old trust even that its not active any more. The package history of a trust is still relevant.
                    var notification = (TrustObsoleteNotification)trustResult.LastOrDefault();
                    notification.OldTrust.TrustPackages.Add(new TrustPackage { PackageID = package.DatabaseID });
                    _db.Update(notification.OldTrust);
                }

                packageResult.Trusts.Add(new TrustAddedResult { ID = trust.Id, Message = trustResult.LastOrDefault()?.ToString() ?? "Unknown" });
            }

            _db.SaveChanges();

            await _notifications.Publish(new PackageAddedNotification { Package = package, Result = packageResult });

            return _notifications;
        }

    }
}
