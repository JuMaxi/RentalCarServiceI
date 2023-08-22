﻿using Microsoft.EntityFrameworkCore;
using RentalCarService.Interfaces;
using RentalCarService.Models;
using RentalCarService.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RentalCarService.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly RentalCarsDBContext _dbcontext;

        public AvailabilityService(RentalCarsDBContext dbContext)
        {
            _dbcontext = dbContext;
        }
        //public bool ExistsAvailabilityForBooking(Booking candidate, List<Booking> nearbyBookings, int amountCarsInCategory = 1)
        //{
        //    foreach (Booking booking in nearbyBookings)
        //    {
        //        //Add 1 hour to give time to clean the car.
        //        if (candidate.ReturnDay.AddHours(1) >= booking.StartDay
        //        && candidate.StartDay <= booking.StartDay)
        //        {
        //            if (amountCarsInCategory < nearbyBookings.Count || amountCarsInCategory == 1)
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        //public bool ExistsAvailabilityForBooking(Booking candidate, List<Booking> nearbyBookings, int amountCarsInCategory = 1)
        //{
        //    int amountBooked = 0;
        //    foreach (Booking booking in nearbyBookings)
        //    {
        //        //Add 1 hour to give time to clean the car.
        //        if (candidate.ReturnDay.AddHours(1) >= booking.StartDay
        //        && candidate.StartDay <= booking.StartDay)
        //        {
        //            amountBooked++;

        //            if (amountBooked == amountCarsInCategory)
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        public bool ExistsAvailabilityForBooking(Booking candidate, List<Booking> nearbyBookings, int amountCarsInCategory = 1)
        {
            DateTime dayCandidate = candidate.StartDay;

            int amountBooked = 0;
            int teste = 0;

            while (dayCandidate <= candidate.ReturnDay)
            {
                for (int i = 0; i < nearbyBookings.Count; i++)
                {
                    if (candidate.ReturnDay.AddHours(1) >= nearbyBookings[i].StartDay
                        && candidate.StartDay <= nearbyBookings[i].StartDay)
                    {
                        amountBooked++;


                        for (int index = i + 1; index < nearbyBookings.Count; index++)
                        {
                            if (nearbyBookings[i].StartDay.Date >= nearbyBookings[index].StartDay.Date
                                && nearbyBookings[i].StartDay.Date <= nearbyBookings[index].ReturnDay.Date)
                            {
                                amountBooked++;
                            }
                        }
                        if (amountBooked > teste)
                        {
                            teste = amountBooked;
                        }
                        amountBooked = 0;
                    }
                }

                candidate.StartDay = candidate.StartDay.AddDays(1);

                dayCandidate = dayCandidate.AddDays(1);
            }

            if (teste >= amountCarsInCategory)
            {
                return false;
            }
            return true;
        }

        private Dictionary<int, int> SumAmountCategoryCarBranch(AvailabilityRequest availability)
        {
            List<Car> fleet = FindCarFromDB(availability.BranchGetCar);

            Dictionary<int, int> carPerCategory = new Dictionary<int, int>();


            foreach (Car car in fleet)
            {
                if (!carPerCategory.ContainsKey(car.Category.Id))
                {
                    carPerCategory[car.Category.Id] = 1;
                }
                else
                {
                    carPerCategory[car.Category.Id] = carPerCategory[car.Category.Id] + 1;
                }
            }

            return carPerCategory;
        }

        private Dictionary<int, int> CompareAvailability(AvailabilityRequest availability)
        {
            Dictionary<int, int> carsCategoryBranch = SumAmountCategoryCarBranch(availability);
            List<Booking> bookedCars = FindBookFromDB(availability);

            Booking booking = new Booking();
            booking.StartDay = availability.StartDay;
            booking.ReturnDay = availability.ReturnDay;

            foreach (KeyValuePair<int, int> c in carsCategoryBranch)
            {
                List<Booking> list = bookedCars.Where(k => k.Category.Id == c.Key).ToList();

                if (list.Count > 0)
                {
                    bool check = ExistsAvailabilityForBooking(booking, list, c.Value);

                    if (check == false)
                    {
                        carsCategoryBranch[c.Key] = c.Value - 1;
                    }
                }
            }

            Dictionary<int, int> newDictionary = carsCategoryBranch.Where(v => v.Value > 0).ToDictionary(k => k.Key, v => v.Value);

            return newDictionary;
        }

        public List<Categories> SaveListAvailableCategories(AvailabilityRequest availability)
        {
            Dictionary<int, int> numberAvailableCategories = CompareAvailability(availability);

            List<Categories> availableCategories = new List<Categories>();
            availableCategories = _dbcontext.Categories
                .Where(cat => numberAvailableCategories.Keys
                .Contains(cat.Id))
                .Include(o => o.PriceBands).ToList();

            return availableCategories;
        }
        private List<Car> FindCarFromDB(int id)
        {
            var fleet = _dbcontext.Fleet
                .Include(c => c.Category)
                .Include(br => br.Branch)
                .ThenInclude(o => o.OpeningHours)
                .Where(b => b.Branch.Id == id)
                .ToList();

            return fleet;
        }

        private List<Booking> FindBookFromDB(AvailabilityRequest availability)
        {
            List<Booking> books = _dbcontext.Books
                .Where(d => d.StartDay.Date <= availability.ReturnDay.Date)
                .Where(c => c.StartDay.Date >= availability.StartDay.Date)
                .Where(b => b.BranchGet.Id == availability.BranchGetCar)
                .ToList();

            return books;
        }
    }
}
