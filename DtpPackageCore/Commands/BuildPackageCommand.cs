﻿using DtpCore.Model;
using DtpCore.Notifications;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class BuildPackageCommand : IRequest<NotificationSegment>
    {
        // Some filtering data, to narrow down the package.
    }
}