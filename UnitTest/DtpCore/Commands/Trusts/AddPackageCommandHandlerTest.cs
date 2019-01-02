﻿using DtpCore.Builders;
using DtpCore.Commands;
using DtpCore.Commands.Trusts;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitTest.DtpCore.Extensions;
using UnitTest.DtpPackage.Mocks;

namespace UnitTest.DtpCore.Commands.Trusts
{
    [TestClass]
    public class AddPackageCommandHandlerTest : StartupMock
    {

        public Package CreatePackage()
        {
            var builder = new PackageBuilder(ServiceProvider);
            builder.BuildBinaryTrust("testissuer1", "testsubject1", true);
            var package = builder.Package;
            NotificationSegment result = Mediator.SendAndWait(new AddPackageCommand { Package = package });
            DB.SaveChanges();
            return package;
        }

        [TestMethod]
        public void Add()
        {
            var builder = new PackageBuilder(ServiceProvider);
            builder.BuildBinaryTrust("testissuer1", "testsubject1", true);
            var package = builder.Package;
            NotificationSegment result = Mediator.SendAndWait(new AddPackageCommand { Package = package });
            DB.SaveChanges();

            Assert.AreEqual(1, result.Count);
            var last = result.Last();
            Assert.IsTrue(last is PackageAddedNotification);
            Assert.IsTrue(DB.Packages.Count() == 1);
            Assert.IsTrue(DB.Claims.Count() == 1);
            Assert.IsTrue(DB.TrustPackages.Count() == 1);
            Assert.IsTrue(DB.Claims.FirstOrDefault().PackageDatabaseID == null); // Should be 0

        }

        //[TestMethod]
        //public void Replace()
        //{
        //    var oldtrust = CreateTrust();

        //    var builder = new TrustBuilder(ServiceProvider);
        //    var trust = builder.BuildBinaryTrust("testissuer1", "testsubject1", false);

        //    NotificationSegment result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust });

        //    Assert.AreEqual(2, result.Count);

        //    Assert.IsTrue(result[0] is TrustReplacedNotification);
        //    Assert.IsTrue(((TrustReplacedNotification)result[0]).Trust.Id == oldtrust.Id);

        //    Assert.IsTrue(result[1] is TrustAddedNotification);
        //    Assert.IsTrue(((TrustAddedNotification)result[1]).Trust.Id == trust.Id);
        //}

        //[TestMethod]
        //public void Exist()
        //{
        //    var trust = CreateTrust();

        //    NotificationSegment result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust });

        //    Assert.AreEqual(1, result.Count);

        //    Assert.IsTrue(result[0] is TrustExistNotification);
        //}

        //[TestMethod]
        //public void Old()
        //{
        //    var newtrust = CreateTrust();

        //    var builder = new TrustBuilder(ServiceProvider);
        //    var oldtrust = builder.BuildBinaryTrust("testissuer1", "testsubject1", true, 1); 

        //    NotificationSegment result = Mediator.SendAndWait(new AddTrustCommand { Trust = oldtrust });

        //    Assert.AreEqual(1, result.Count);

        //    Assert.IsTrue(result[0] is TrustObsoleteNotification);
        //}


    }
}
