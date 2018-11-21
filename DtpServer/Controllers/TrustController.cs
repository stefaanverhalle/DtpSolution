﻿using System.Linq;
using DtpCore.Model;
using Microsoft.AspNetCore.Mvc;
using DtpGraphCore.Interfaces;
using DtpCore.Controllers;
using DtpCore.Interfaces;
using DtpStampCore.Interfaces;
using System;
using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using System.Collections.Generic;
using MediatR;
using DtpCore.Commands;
using DtpCore.Commands.Trusts;
using DtpCore.ViewModel;
using DtpCore.Notifications;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Handles trust related stuff.
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class TrustController : ApiController
    {
        private IMediator _mediator;

        private ITrustSchemaService _trustSchemaService;
        private ITrustDBService _trustDBService;
        private IServiceProvider _serviceProvider;

        public TrustController(IMediator mediator, ITrustSchemaService trustSchemaService, ITrustDBService trustDBService, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _trustSchemaService = trustSchemaService;
            _trustDBService = trustDBService;
            _serviceProvider = serviceProvider;
        }





        /// <summary>
        /// Add a package to the Graph and database.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        [Produces("application/json")]
        [HttpPost]
        [Route("add")]
        public ActionResult Add([FromBody]Package package)
        {
            var validationResult = _trustSchemaService.Validate(package, TrustSchemaValidationOptions.Full);
            if (validationResult.ErrorsFound > 0)
                return ApiError(validationResult, null, "Validation failed");
            // Timestamp validation service disabled for the moment

            var result = _mediator.SendAndWait(new AddPackageCommand { Package = package });

            return ApiOk(result.LastOrDefault());
        }

        /// <summary>
        /// Create a trust, that is not added but returned for signing.
        /// </summary>
        /// <returns>A newly created trust object</returns>
        [HttpGet]
        [Route("build")]
        public ActionResult<Trust> BuildTrust(string issuer, string subject, string issuerScript = "", string type = TrustBuilder.BINARY_TRUST_DTP1, string attributes = "", string scope = "", string alias = "")
        {
            if (issuer == null || issuer.Length < 1)
                throw new ApplicationException("Missing issuer");

            if (subject == null || subject.Length < 1)
                throw new ApplicationException("Missing subject");

            if (string.IsNullOrEmpty(attributes))
                if (type == TrustBuilder.BINARY_TRUST_DTP1)
                    attributes = TrustBuilder.CreateBinaryTrustAttributes();

            var trustBuilder = new TrustBuilder(_serviceProvider);
            trustBuilder.AddTrust()
                .SetIssuer(issuer, issuerScript)
                .AddType(type, attributes)
                .AddSubject(subject)
                .BuildTrustID();

            return trustBuilder.CurrentTrust;
        }


        /// <summary>
        /// Build a package for the client to sign.
        /// </summary>
        /// <remarks>
        /// A client can add the trusts to a package and this function will build the package.
        /// The client however still needs to sign the package.
        /// </remarks>
        /// <param name="package"></param>
        /// <returns>A package with calculated id</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the package do not validate</response>    
        [HttpPost]
        [Route("build")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public ActionResult<Package> BuildTrust([FromBody]Package package)
        {
            var validationResult = _trustSchemaService.Validate(package, TrustSchemaValidationOptions.Basic);
            if (validationResult.ErrorsFound > 0)
                return ApiError(validationResult, null, "Validation failed");

            var trustBuilder = new TrustBuilder(_serviceProvider)
            {
                Package = package
            };
            trustBuilder.Build();

            return ApiOk(package);
        }

        [HttpGet]
        //[Route("get/{trustId}")]
        public ActionResult Get([FromRoute]byte[] trustId)
        {
            if (trustId == null || trustId.Length < 1)
                throw new ApplicationException("Missing trustId");

            var trust = _trustDBService.GetTrustById(trustId);

            return ApiOk(trust);
        }

        [HttpGet]
        [Route("get")]
        public ActionResult Get([FromQuery]string issuer, [FromQuery]string subject, [FromQuery]string type, [FromQuery]string scopevalue)
        {
            var query = new Trust
            {
                Issuer = new IssuerIdentity { Address = issuer },
                Subject = new SubjectIdentity { Address = subject },
                Type = type,
                Scope = scopevalue
            };

            var trust = _trustDBService.GetSimilarTrust(query);

            return ApiOk(trust);
        }
    }
}
