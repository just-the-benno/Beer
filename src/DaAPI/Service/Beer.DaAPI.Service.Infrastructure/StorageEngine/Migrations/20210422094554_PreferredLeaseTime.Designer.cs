﻿// <auto-generated />
using System;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    [DbContext(typeof(StorageContext))]
    [Migration("20210422094554_PreferredLeaseTime")]
    partial class PreferredLeaseTime
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.4")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4.DHCPv4InterfaceDataModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("IPv4Address")
                        .HasColumnType("text");

                    b.Property<string>("InterfaceId")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DHCPv4Interfaces");
                });

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4.DHCPv4LeaseEntryDataModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("EndOfPreferredLifetime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("EndOfRenewalTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("EndReason")
                        .HasColumnType("integer");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<Guid>("LeaseId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ScopeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("DHCPv4LeaseEntries");
                });

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4.DHCPv4PacketHandledEntryDataModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ErrorCode")
                        .HasColumnType("integer");

                    b.Property<string>("FilteredBy")
                        .HasColumnType("text");

                    b.Property<bool>("HandledSuccessfully")
                        .HasColumnType("boolean");

                    b.Property<bool>("InvalidRequest")
                        .HasColumnType("boolean");

                    b.Property<string>("RequestDestination")
                        .HasColumnType("text");

                    b.Property<int>("RequestSize")
                        .HasColumnType("integer");

                    b.Property<string>("RequestSource")
                        .HasColumnType("text");

                    b.Property<byte[]>("RequestStream")
                        .HasColumnType("bytea");

                    b.Property<int>("RequestType")
                        .HasColumnType("integer");

                    b.Property<string>("ResponseDestination")
                        .HasColumnType("text");

                    b.Property<int?>("ResponseSize")
                        .HasColumnType("integer");

                    b.Property<string>("ResponseSource")
                        .HasColumnType("text");

                    b.Property<byte[]>("ResponseStream")
                        .HasColumnType("bytea");

                    b.Property<int?>("ResponseType")
                        .HasColumnType("integer");

                    b.Property<Guid?>("ScopeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("TimestampDay")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("TimestampMonth")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("TimestampWeek")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("DHCPv4PacketEntries");
                });

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6.DHCPv6InterfaceDataModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("IPv6Address")
                        .HasColumnType("text");

                    b.Property<string>("InterfaceId")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DHCPv6Interfaces");
                });

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6.DHCPv6LeaseEntryDataModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("EndOfPreferredLifetime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("EndOfRenewalTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("EndReason")
                        .HasColumnType("integer");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<Guid>("LeaseId")
                        .HasColumnType("uuid");

                    b.Property<string>("Prefix")
                        .HasColumnType("text");

                    b.Property<byte>("PrefixLength")
                        .HasColumnType("smallint");

                    b.Property<Guid>("ScopeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("DHCPv6LeaseEntries");
                });

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6.DHCPv6PacketHandledEntryDataModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ErrorCode")
                        .HasColumnType("integer");

                    b.Property<string>("FilteredBy")
                        .HasColumnType("text");

                    b.Property<bool>("HandledSuccessfully")
                        .HasColumnType("boolean");

                    b.Property<bool>("InvalidRequest")
                        .HasColumnType("boolean");

                    b.Property<string>("RequestDestination")
                        .HasColumnType("text");

                    b.Property<int>("RequestSize")
                        .HasColumnType("integer");

                    b.Property<string>("RequestSource")
                        .HasColumnType("text");

                    b.Property<byte[]>("RequestStream")
                        .HasColumnType("bytea");

                    b.Property<byte>("RequestType")
                        .HasColumnType("smallint");

                    b.Property<string>("ResponseDestination")
                        .HasColumnType("text");

                    b.Property<int?>("ResponseSize")
                        .HasColumnType("integer");

                    b.Property<string>("ResponseSource")
                        .HasColumnType("text");

                    b.Property<byte[]>("ResponseStream")
                        .HasColumnType("bytea");

                    b.Property<byte?>("ResponseType")
                        .HasColumnType("smallint");

                    b.Property<Guid?>("ScopeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("TimestampDay")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("TimestampMonth")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("TimestampWeek")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("DHCPv6PacketEntries");
                });

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.HelperEntry", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.ToTable("Helper");
                });

            modelBuilder.Entity("Beer.DaAPI.Infrastructure.StorageEngine.NotificationPipelineOverviewEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("NotificationPipelineEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
