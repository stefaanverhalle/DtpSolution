﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpGraphCore.Interfaces;
using UnitTest.DtpCore.Extensions;
using Newtonsoft.Json;
using System;
using DtpGraphCore.Builders;
using System.Collections.Generic;
using DtpGraphCore.Model;
using DtpCore.Model;
using DtpGraphCore.Extensions;
using DtpGraphCore.Controllers;
using DtpServer.Controllers;

namespace UnitTest.DtpGraphCore
{
    public class TrustGraphMock : StartupMock
    {
        protected IGraphClaimService _graphTrustService { get; set; }

        protected PackageBuilder _trustBuilder { get; set; } 

        protected ITrustDBService _trustDBService { get; set; } 

        protected IGraphQueryService _graphQueryService { get; set; } 

        protected TrustController _trustController { get; set; } 

        protected IGraphLoadSaveService _graphLoadSaveService { get; set; } 

        protected string BinaryTrustTrueAttributes { get; set; } 

        protected string BinaryTrustFalseAttributes { get; set; } 

        protected string ConfirmAttributes { get; set; }

        protected string RatingAtrributes { get; set; } 

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _graphTrustService = ServiceProvider.GetRequiredService<IGraphClaimService>();
            _trustBuilder = new PackageBuilder(ServiceProvider);
            _trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            //_graphQueryService = new GraphQueryService(_graphTrustService);
            _graphQueryService = ServiceProvider.GetRequiredService<IGraphQueryService>();
            _trustController = ServiceProvider.GetRequiredService<TrustController>();
            _graphLoadSaveService = ServiceProvider.GetRequiredService<IGraphLoadSaveService>();

            BinaryTrustTrueAttributes = PackageBuilder.CreateBinaryTrustAttributes(true);
            BinaryTrustFalseAttributes = PackageBuilder.CreateBinaryTrustAttributes(false);
            ConfirmAttributes = PackageBuilder.CreateConfirmAttributes();
            RatingAtrributes = PackageBuilder.CreateRatingAttributes(100);
        }


        protected void PrintJson(object data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Console.WriteLine(json);
        }

        protected QueryRequest BuildQuery(QueryRequestBuilder queryBuilder, string source, string target)
        {
            var sourceAddress = TrustBuilderExtensions.GetAddress(source);
            var subject = new Identity
            {
                Id = TrustBuilderExtensions.GetAddress(target)
            };
            queryBuilder.Add(sourceAddress, subject.Id);

            return queryBuilder.Query;
        }
        protected void VerfifyContext(QueryContext context, int exspectedResults, int exspcetedErrors = 0)
        {
            Assert.AreEqual(exspcetedErrors, context.Errors.Count, $"{string.Join("\r\n", context.Errors.ToArray())}");
            Assert.AreEqual(exspectedResults, context.Results.Claims.Count, $"Should be {exspectedResults} results!");

        }

        protected void VerfifyResult(QueryContext context, string source, string target, string type = "")
        {
            var sourceAddress = TrustBuilderExtensions.GetAddress(source);
            var targetAddress = TrustBuilderExtensions.GetAddress(target);
            var sourceIndex = _graphTrustService.Graph.IssuerIndex.GetValueOrDefault(sourceAddress);
            var targetIndex = _graphTrustService.Graph.IssuerIndex.GetValueOrDefault(targetAddress);

            var tracker = context.TrackerResults.GetValueOrDefault(sourceIndex);
            Assert.IsNotNull(tracker, $"Result is missing source: {source}");

            var subject = tracker.Subjects.GetValueOrDefault(targetIndex);
            Assert.IsNotNull(subject, $"Result is missing for subject for: {source} - subject: {target}");

            //if (trustClaim != null)
            //{
            //    var graphClaim = _graphTrustService.CreateGraphClaim(trustClaim);
            //    var exist = subject.Claims.Exist(graphClaim.Scope, graphClaim.Type);
            //    Assert.IsTrue(exist, "Subject missing the claim type: " + trustClaim.Type);
            //}
        }

        protected void BuildTestGraph()
        {
            _trustBuilder.SetServer("testserver");

            _trustBuilder.AddTrust("A", "B", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("B", "C", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("C", "D", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("B", "E", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("E", "D", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("B", "F", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("F", "G", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("G", "D", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes); // Long way, no trust
            _trustBuilder.AddTrust("G", "Unreach", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes); // Long way, no trust

            _trustBuilder.AddTrust("A", "B", PackageBuilder.CONFIRM_TRUST_DTP1, ConfirmAttributes);
            _trustBuilder.AddTrust("C", "D", PackageBuilder.CONFIRM_TRUST_DTP1, ConfirmAttributes);
            _trustBuilder.AddTrust("G", "D", PackageBuilder.CONFIRM_TRUST_DTP1, ConfirmAttributes);

            _trustBuilder.AddTrust("A", "B", PackageBuilder.RATING_TRUST_DTP1, RatingAtrributes);
            _trustBuilder.AddTrust("C", "D", PackageBuilder.RATING_TRUST_DTP1, RatingAtrributes);
            _trustBuilder.AddTrust("G", "D", PackageBuilder.RATING_TRUST_DTP1, RatingAtrributes);

            _trustBuilder.AddTrust("A", "NoTrustB", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);
            _trustBuilder.AddTrust("B", "NoTrustC", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);
            _trustBuilder.AddTrust("C", "NoTrustD", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);

            _trustBuilder.AddTrust("C", "MixD", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("E", "MixD", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);

            _trustBuilder.Build().Sign();
        }

        protected void EnsureTestGraph()
        {
            BuildTestGraph();
            _graphTrustService.Add(_trustBuilder.Package);

        }

    }
}
