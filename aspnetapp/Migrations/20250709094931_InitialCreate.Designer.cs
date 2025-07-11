﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using aspnetapp;

#nullable disable

namespace aspnetapp.Migrations
{
    [DbContext(typeof(GuardersContext))]
    [Migration("20250709094931_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("aspnetapp.EF.GuarderDB", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("GuarderType")
                        .HasColumnType("int");

                    b.Property<int>("GuarderKakera")
                        .HasColumnType("int");

                    b.Property<short>("GuarderLevel")
                        .HasColumnType("smallint");

                    b.HasKey("UserId", "GuarderType");

                    b.ToTable("Guarders");
                });

            modelBuilder.Entity("aspnetapp.EF.TreasureBoxDB", b =>
                {
                    b.Property<string>("TreasureBoxId")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("TreasureBoxCreateAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("TreasureBoxType")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("TreasureBoxId");

                    b.HasIndex("UserId");

                    b.ToTable("TreasureBoxs");
                });

            modelBuilder.Entity("aspnetapp.EF.UserDB", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("UniqueId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<short>("UserAP")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("UserCreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<long>("UserGold")
                        .HasColumnType("bigint");

                    b.Property<short>("UserLevel")
                        .HasColumnType("smallint");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserXP")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("QuestionDB", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("QuestionID")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("QuestionBestTime")
                        .HasColumnType("int");

                    b.Property<short>("QuestionStar")
                        .HasColumnType("smallint");

                    b.HasKey("UserId", "QuestionID");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("aspnetapp.EF.GuarderDB", b =>
                {
                    b.HasOne("aspnetapp.EF.UserDB", "User")
                        .WithMany("Guarders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("aspnetapp.EF.TreasureBoxDB", b =>
                {
                    b.HasOne("aspnetapp.EF.UserDB", "User")
                        .WithMany("TreasureBoxs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("QuestionDB", b =>
                {
                    b.HasOne("aspnetapp.EF.UserDB", "User")
                        .WithMany("Questions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("aspnetapp.EF.UserDB", b =>
                {
                    b.Navigation("Guarders");

                    b.Navigation("Questions");

                    b.Navigation("TreasureBoxs");
                });
#pragma warning restore 612, 618
        }
    }
}
