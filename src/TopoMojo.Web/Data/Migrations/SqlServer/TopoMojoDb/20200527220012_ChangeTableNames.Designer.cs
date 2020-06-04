// Copyright 2020 Carnegie Mellon University. 
// Released under a MIT (SEI) license. See LICENSE.md in the project root. 

﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TopoMojo.Data;

namespace TopoMojo.Web.Data.Migrations.SqlServer.TopoMojoDb
{
    [DbContext(typeof(TopoMojoDbContextSqlServer))]
    [Migration("20200527220012_ChangeTableNames")]
    partial class ChangeTableNames
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TopoMojo.Data.Activity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Action")
                        .HasColumnType("int");

                    b.Property<string>("Actor")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ActorId")
                        .HasColumnType("int");

                    b.Property<string>("Annotation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Asset")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("AssetId")
                        .HasColumnType("int");

                    b.Property<DateTime>("At")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("History");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("ShareCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.Property<int>("WorkspaceId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Gamespaces");
                });

            modelBuilder.Entity("TopoMojo.Data.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AuthorId")
                        .HasColumnType("int");

                    b.Property<string>("AuthorName")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<bool>("Edited")
                        .HasColumnType("bit");

                    b.Property<string>("RoomId")
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(2048)")
                        .HasMaxLength(2048);

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("GamespaceId")
                        .HasColumnType("int");

                    b.Property<int>("Permission")
                        .HasColumnType("int");

                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GamespaceId");

                    b.HasIndex("PersonId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("Detail")
                        .HasColumnType("nvarchar(2048)")
                        .HasMaxLength(2048);

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<string>("Guestinfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("bit");

                    b.Property<string>("Iso")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Networks")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<int?>("ParentId")
                        .HasColumnType("int");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.Property<int?>("WorkspaceId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("ParentId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("TopoMojo.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.Property<int>("WorkspaceLimit")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TopoMojo.Data.Worker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Permission")
                        .HasColumnType("int");

                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.Property<int>("WorkspaceId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("TopoMojo.Data.Workspace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Audience")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Author")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("DocumentUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<bool>("IsPublished")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("datetime2");

                    b.Property<int>("LaunchCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("ShareCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TemplateLimit")
                        .HasColumnType("int");

                    b.Property<bool>("UseUplinkSwitch")
                        .HasColumnType("bit");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.ToTable("Workspaces");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.HasOne("TopoMojo.Data.Workspace", "Workspace")
                        .WithMany("Gamespaces")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.HasOne("TopoMojo.Data.Gamespace", "Gamespace")
                        .WithMany("Players")
                        .HasForeignKey("GamespaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TopoMojo.Data.User", "Person")
                        .WithMany("Gamespaces")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.HasOne("TopoMojo.Data.Template", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.HasOne("TopoMojo.Data.Workspace", "Workspace")
                        .WithMany("Templates")
                        .HasForeignKey("WorkspaceId");
                });

            modelBuilder.Entity("TopoMojo.Data.Worker", b =>
                {
                    b.HasOne("TopoMojo.Data.User", "Person")
                        .WithMany("Workspaces")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TopoMojo.Data.Workspace", "Workspace")
                        .WithMany("Workers")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
