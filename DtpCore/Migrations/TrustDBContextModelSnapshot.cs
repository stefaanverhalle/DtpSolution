﻿// <auto-generated />
using System;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DtpCore.Migrations
{
    [DbContext(typeof(TrustDBContext))]
    partial class TrustDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity("DtpCore.Model.BlockchainProof", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<long>("BlockTime");

                    b.Property<string>("Blockchain");

                    b.Property<int>("Confirmations");

                    b.Property<byte[]>("MerkleRoot");

                    b.Property<byte[]>("Receipt");

                    b.Property<int>("RetryAttempts");

                    b.Property<string>("Status");

                    b.HasKey("DatabaseID");

                    b.ToTable("BlockchainProof");
                });

            modelBuilder.Entity("DtpCore.Model.Claim", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Activate");

                    b.Property<string>("Algorithm");

                    b.Property<uint>("Created");

                    b.Property<uint>("Expire");

                    b.Property<byte[]>("Id");

                    b.Property<string>("Note");

                    b.Property<byte[]>("Root");

                    b.Property<string>("Scope");

                    b.Property<long>("State");

                    b.Property<string>("Type");

                    b.Property<string>("Value");

                    b.HasKey("DatabaseID");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Claim");
                });

            modelBuilder.Entity("DtpCore.Model.ClaimPackageRelationship", b =>
                {
                    b.Property<int?>("ClaimID");

                    b.Property<int?>("PackageID");

                    b.HasKey("ClaimID", "PackageID");

                    b.HasIndex("PackageID");

                    b.ToTable("ClaimPackageRelationship");
                });

            modelBuilder.Entity("DtpCore.Model.KeyValue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<byte[]>("Value");

                    b.HasKey("ID");

                    b.HasIndex("Key");

                    b.ToTable("KeyValues");
                });

            modelBuilder.Entity("DtpCore.Model.Package", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Algorithm");

                    b.Property<uint>("Created");

                    b.Property<string>("File");

                    b.Property<byte[]>("Id");

                    b.Property<string>("Obsoletes");

                    b.Property<byte[]>("Root");

                    b.Property<string>("Scopes");

                    b.Property<long>("State");

                    b.Property<string>("Types");

                    b.HasKey("DatabaseID");

                    b.HasIndex("Id");

                    b.ToTable("Package");
                });

            modelBuilder.Entity("DtpCore.Model.Timestamp", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Algorithm");

                    b.Property<string>("Blockchain");

                    b.Property<int?>("ClaimDatabaseID");

                    b.Property<int>("PackageDatabaseID");

                    b.Property<int?>("ProofDatabaseID");

                    b.Property<long>("Registered");

                    b.Property<string>("Service");

                    b.Property<byte[]>("Source");

                    b.Property<byte[]>("Value");

                    b.HasKey("DatabaseID");

                    b.HasIndex("ClaimDatabaseID");

                    b.HasIndex("PackageDatabaseID");

                    b.HasIndex("ProofDatabaseID");

                    b.HasIndex("Source");

                    b.ToTable("Timestamp");
                });

            modelBuilder.Entity("DtpCore.Model.WorkflowContainer", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<string>("Data");

                    b.Property<long>("NextExecution");

                    b.Property<string>("State");

                    b.Property<string>("Tag");

                    b.Property<string>("Type");

                    b.HasKey("DatabaseID");

                    b.HasIndex("State");

                    b.HasIndex("Type");

                    b.ToTable("Workflow");
                });

            modelBuilder.Entity("DtpCore.Model.Claim", b =>
                {
                    b.OwnsOne("DtpCore.Model.IssuerIdentity", "Issuer", b1 =>
                        {
                            b1.Property<int>("ClaimDatabaseID");

                            b1.Property<string>("Id");

                            b1.Property<byte[]>("Signature");

                            b1.Property<string>("Type");

                            b1.HasKey("ClaimDatabaseID");

                            b1.HasIndex("Id");

                            b1.ToTable("Claim");

                            b1.HasOne("DtpCore.Model.Claim")
                                .WithOne("Issuer")
                                .HasForeignKey("DtpCore.Model.IssuerIdentity", "ClaimDatabaseID")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("DtpCore.Model.SubjectIdentity", "Subject", b1 =>
                        {
                            b1.Property<int>("ClaimDatabaseID");

                            b1.Property<string>("Id");

                            b1.Property<byte[]>("Signature");

                            b1.Property<string>("Type");

                            b1.HasKey("ClaimDatabaseID");

                            b1.HasIndex("Id");

                            b1.ToTable("Claim");

                            b1.HasOne("DtpCore.Model.Claim")
                                .WithOne("Subject")
                                .HasForeignKey("DtpCore.Model.SubjectIdentity", "ClaimDatabaseID")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("DtpCore.Model.ClaimPackageRelationship", b =>
                {
                    b.HasOne("DtpCore.Model.Claim", "Claim")
                        .WithMany("ClaimPackages")
                        .HasForeignKey("ClaimID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DtpCore.Model.Package", "Package")
                        .WithMany("ClaimPackages")
                        .HasForeignKey("PackageID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DtpCore.Model.Package", b =>
                {
                    b.OwnsOne("DtpCore.Model.ServerIdentity", "Server", b1 =>
                        {
                            b1.Property<int>("PackageDatabaseID");

                            b1.Property<string>("Id");

                            b1.Property<byte[]>("Signature");

                            b1.Property<string>("Type");

                            b1.HasKey("PackageDatabaseID");

                            b1.ToTable("Package");

                            b1.HasOne("DtpCore.Model.Package")
                                .WithOne("Server")
                                .HasForeignKey("DtpCore.Model.ServerIdentity", "PackageDatabaseID")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("DtpCore.Model.Timestamp", b =>
                {
                    b.HasOne("DtpCore.Model.Claim")
                        .WithMany("Timestamps")
                        .HasForeignKey("ClaimDatabaseID");

                    b.HasOne("DtpCore.Model.Package")
                        .WithMany("Timestamps")
                        .HasForeignKey("PackageDatabaseID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DtpCore.Model.BlockchainProof", "Proof")
                        .WithMany("Timestamps")
                        .HasForeignKey("ProofDatabaseID");
                });
#pragma warning restore 612, 618
        }
    }
}
