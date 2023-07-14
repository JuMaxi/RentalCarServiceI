﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Hosting;
using RentalCarService.Models;
using System;

namespace RentalCarService
{
    public class RentalCarsDBContext : DbContext
    {
        public RentalCarsDBContext(DbContextOptions<RentalCarsDBContext> options) : base(options)
        {
        }

        public DbSet<Countries> Countries { get; set; }
        public DbSet<Brands> Brands { get; set; }

        public DbSet<Categories> Categories { get; set; }
        public DbSet<Branchs> Branches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OpeningHours>(builder =>
            {
                // Time is a TimeOnly property and time on database
                builder.Property(x => x.Opens)
                    .HasConversion<TimeOnlyDbConverter, TimeOnlyComparer>();

                builder.Property(x => x.Closes)
                    .HasConversion<TimeOnlyDbConverter, TimeOnlyComparer>();
            });
        }

    }
    public class TimeOnlyDbConverter : ValueConverter<TimeOnly, TimeSpan>
    {
        public TimeOnlyDbConverter() : base(
                timeOnly => timeOnly.ToTimeSpan(),
                timeSpan => TimeOnly.FromTimeSpan(timeSpan))
        {
        }
    }

    public class TimeOnlyComparer : ValueComparer<TimeOnly>
    {
        public TimeOnlyComparer() : base(
            (t1, t2) => t1.Ticks == t2.Ticks,
            t => t.GetHashCode())
        {
        }
    }
}