﻿using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpCore.Commands.Trusts
{
    public class AddTrustCommand : IRequest<NotificationSegment>
    {
        public Trust Trust { get; set; }
    }
}
