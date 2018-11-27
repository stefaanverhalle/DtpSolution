﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DtpCore.Migrations
{
    public partial class ID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockchainProof",
                columns: table => new
                {
                    DatabaseID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Blockchain = table.Column<string>(nullable: true),
                    MerkleRoot = table.Column<byte[]>(nullable: true),
                    Receipt = table.Column<byte[]>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Confirmations = table.Column<int>(nullable: false),
                    BlockTime = table.Column<long>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    RetryAttempts = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainProof", x => x.DatabaseID);
                });

            migrationBuilder.CreateTable(
                name: "KeyValues",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValues", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Package",
                columns: table => new
                {
                    DatabaseID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Algorithm = table.Column<string>(nullable: true),
                    Id = table.Column<byte[]>(nullable: false),
                    Created = table.Column<uint>(nullable: false),
                    Server_Type = table.Column<string>(nullable: true),
                    Server_Id = table.Column<string>(nullable: true),
                    Server_Signature = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.DatabaseID);
                    table.UniqueConstraint("AK_Package_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workflow",
                columns: table => new
                {
                    DatabaseID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    State = table.Column<string>(nullable: true),
                    Tag = table.Column<string>(nullable: true),
                    NextExecution = table.Column<long>(nullable: false),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow", x => x.DatabaseID);
                });

            migrationBuilder.CreateTable(
                name: "Trust",
                columns: table => new
                {
                    DatabaseID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Algorithm = table.Column<string>(nullable: true),
                    Id = table.Column<byte[]>(nullable: true),
                    Created = table.Column<uint>(nullable: false),
                    Issuer_Type = table.Column<string>(nullable: true),
                    Issuer_Id = table.Column<string>(nullable: true),
                    Issuer_Signature = table.Column<byte[]>(nullable: true),
                    Subject_Type = table.Column<string>(nullable: true),
                    Subject_Id = table.Column<string>(nullable: true),
                    Subject_Signature = table.Column<byte[]>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Claim = table.Column<string>(nullable: true),
                    Scope = table.Column<string>(nullable: true),
                    Activate = table.Column<uint>(nullable: false),
                    Expire = table.Column<uint>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    PackageDatabaseID = table.Column<int>(nullable: true),
                    Replaced = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trust", x => x.DatabaseID);
                    table.ForeignKey(
                        name: "FK_Trust_Package_PackageDatabaseID",
                        column: x => x.PackageDatabaseID,
                        principalTable: "Package",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Timestamp",
                columns: table => new
                {
                    DatabaseID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Blockchain = table.Column<string>(nullable: true),
                    Algorithm = table.Column<string>(nullable: true),
                    Service = table.Column<string>(nullable: true),
                    Source = table.Column<byte[]>(nullable: true),
                    Receipt = table.Column<byte[]>(nullable: true),
                    Registered = table.Column<long>(nullable: false),
                    BlockchainProof_db_ID = table.Column<int>(nullable: false),
                    PackageDatabase_db_ID = table.Column<int>(nullable: false),
                    TrustDatabase_db_ID = table.Column<int>(nullable: false),
                    BlockchainProofDatabaseID = table.Column<int>(nullable: true),
                    PackageDatabaseID = table.Column<int>(nullable: true),
                    TrustDatabaseID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timestamp", x => x.DatabaseID);
                    table.ForeignKey(
                        name: "FK_Timestamp_BlockchainProof_BlockchainProofDatabaseID",
                        column: x => x.BlockchainProofDatabaseID,
                        principalTable: "BlockchainProof",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Timestamp_Package_PackageDatabaseID",
                        column: x => x.PackageDatabaseID,
                        principalTable: "Package",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Timestamp_Trust_TrustDatabaseID",
                        column: x => x.TrustDatabaseID,
                        principalTable: "Trust",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrustPackage",
                columns: table => new
                {
                    TrustID = table.Column<int>(nullable: false),
                    PackageID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustPackage", x => new { x.TrustID, x.PackageID });
                    table.ForeignKey(
                        name: "FK_TrustPackage_Package_PackageID",
                        column: x => x.PackageID,
                        principalTable: "Package",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrustPackage_Trust_TrustID",
                        column: x => x.TrustID,
                        principalTable: "Trust",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyValues_Key",
                table: "KeyValues",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_Timestamp_BlockchainProofDatabaseID",
                table: "Timestamp",
                column: "BlockchainProofDatabaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Timestamp_BlockchainProof_db_ID",
                table: "Timestamp",
                column: "BlockchainProof_db_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Timestamp_PackageDatabaseID",
                table: "Timestamp",
                column: "PackageDatabaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Timestamp_Source",
                table: "Timestamp",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Timestamp_TrustDatabaseID",
                table: "Timestamp",
                column: "TrustDatabaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Trust_Issuer_Id",
                table: "Trust",
                column: "Issuer_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Trust_Subject_Id",
                table: "Trust",
                column: "Subject_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Trust_Id",
                table: "Trust",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trust_PackageDatabaseID",
                table: "Trust",
                column: "PackageDatabaseID");

            migrationBuilder.CreateIndex(
                name: "IX_TrustPackage_PackageID",
                table: "TrustPackage",
                column: "PackageID");

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_State",
                table: "Workflow",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_Type",
                table: "Workflow",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyValues");

            migrationBuilder.DropTable(
                name: "Timestamp");

            migrationBuilder.DropTable(
                name: "TrustPackage");

            migrationBuilder.DropTable(
                name: "Workflow");

            migrationBuilder.DropTable(
                name: "BlockchainProof");

            migrationBuilder.DropTable(
                name: "Trust");

            migrationBuilder.DropTable(
                name: "Package");
        }
    }
}