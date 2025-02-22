﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WPass.Core;

#nullable disable

namespace WPass.Core.Migrations
{
    [DbContext(typeof(WPContext))]
    [Migration("20240925143942_OneShot")]
    partial class OneShot
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("WPass.Core.Model.BrowserElement", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("BrowserElements");
                });

            modelBuilder.Entity("WPass.Core.Model.Entry", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("CHAR");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("DATETIME");

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("BIT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("NVARCHAR");

                    b.HasKey("Id");

                    b.ToTable("Entries");
                });

            modelBuilder.Entity("WPass.Core.Model.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(100)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("WPass.Core.Model.Website", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("CHAR");

                    b.Property<string>("EntryId")
                        .HasColumnType("CHAR");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("EntryId");

                    b.ToTable("Websites");
                });

            modelBuilder.Entity("WPass.Core.Model.Website", b =>
                {
                    b.HasOne("WPass.Core.Model.Entry", "Entry")
                        .WithMany("Websites")
                        .HasForeignKey("EntryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Entry");
                });

            modelBuilder.Entity("WPass.Core.Model.Entry", b =>
                {
                    b.Navigation("Websites");
                });
#pragma warning restore 612, 618
        }
    }
}
