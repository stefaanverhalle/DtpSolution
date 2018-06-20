﻿using Microsoft.Extensions.DependencyInjection;
using DtpCore.Interfaces;
using DtpCore.Strategy;
using DtpStampCore.Factories;
using DtpStampCore.Interfaces;
using DtpStampCore.Repository;
using DtpStampCore.Services;
using DtpStampCore.Workflows;

namespace DtpStampCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpStrampCore(this IServiceCollection services)
        {
            services.AddSingleton<ITimestampSynchronizationService, TimestampSynchronizationService>();

            services.AddTransient<ITimestampWorkflowService, TimestampWorkflowService>();
            services.AddTransient<ITimestampService, TimestampService>();

            services.AddTransient<IHashAlgorithm, Double256>();
            services.AddTransient<IMerkleTree, MerkleTreeSorted>();

            services.AddTransient<TimestampScheduleWorkflow>();
            services.AddTransient<TimestampWorkflow>();
            services.AddTransient<IBlockchainRepository, SoChainTransactionRepository>();
            services.AddTransient<IBlockchainService, BitcoinService>();

            services.AddTransient<IBlockchainServiceFactory, BlockchainServiceFactory>();
            services.AddTransient<IBlockchainProofFactory, BlockchainProofFactory>();

            services.AddTransient<BitcoinService>();
            services.AddTransient<BitcoinTestService>();
        }
    }
}