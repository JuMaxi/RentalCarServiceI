﻿using Microsoft.AspNetCore.Mvc;
using RentalCarService.Interfaces;
using RentalCarService.Models;
using RentalCarService.Models.Requests;
using RentalCarService.Models.Responses;
using System.Collections.Generic;

namespace RentalCarService.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CarController : ControllerBase
    {
        ICarService CarService;
        public CarController(ICarService carservice) 
        {
            CarService = carservice;
        }

        [HttpPost]
        public void InsertNewCarDB(CarRequest carRequest)
        {
            Car newCar= ConvertCarRequest(carRequest);
            CarService.InsertNewCar(newCar);
        }

        [HttpGet]
        public List<CarResponse> ReadFleetFromDB()
        {
            List<Car> Fleet = CarService.ReadFleetFromDB();
            List<CarResponse> fleetResponse= ConvertCarResponse(Fleet);
            return fleetResponse;
        }

        [HttpDelete]
        public void DeleteCarDB([FromQuery] int Id)
        {
            CarService.DeleteCarFleet(Id);
        }

        [HttpPut]
        public void UpdataCarDb(Car Car)
        {
            CarService.UpdateCarFleet(Car);
        }

        private Car ConvertCarRequest(CarRequest carRequest)
        {
            Car car= new Car();

            Brands brand= new Brands();
            brand.Id= carRequest.BrandId;
            car.Brand= brand;

            car.Model= carRequest.Model;
            car.Year= carRequest.Year;
            car.Transmission= carRequest.Transmission;
            car.Doors= carRequest.Doors;
            car.Seats= carRequest.Seats;
            car.AirConditioner= carRequest.AirConditioner;
            car.TrunkSize= carRequest.TrunkSize;
            car.NumberPlate= carRequest.NumberPlate;

            Categories category= new Categories();
            category.Id= carRequest.CategoryId;
            car.Category= category;

            Branchs branch= new Branchs();
            branch.Id=carRequest.BranchId;
            car.Branch= branch;

            return car;
        }

        private List<CarResponse> ConvertCarResponse(List<Car> cars)
        {
            List<CarResponse> carsResponse= new List<CarResponse>();

            foreach(Car c in cars)
            {
                CarResponse car= new CarResponse();

                car.Brand= c.Brand.Brand;
                car.Model= c.Model;
                car.Year= c.Year;
                car.Transmission= c.Transmission;
                car.Doors= c.Doors;
                car.Seats= c.Seats;
                car.AirConditioner= c.AirConditioner;
                car.TrunkSize= c.TrunkSize;
                car.NumberPlate= c.NumberPlate;
                car.Category = c.Category.Description;
                car.Branch = c.Branch.Name;

                carsResponse.Add(car);
            }

            return carsResponse;
        }
    }
}
