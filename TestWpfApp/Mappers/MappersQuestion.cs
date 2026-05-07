using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TestWpfApp.Data.DataModels;
using TestWpfApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestWpfApp.Mappers
{
    public class MappersQuestion : Profile
    {
        //public MappersQuestion()
        //{
        //    CreateMap<TestQuestion, TestQuestionDataModel>().ReverseMap();
        //}
        public MappersQuestion() 
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            CreateMap<TestQuestionDataModel, TestQuestion>().ForMember(
                dest => dest.FullImageQuestion, 
                opt => opt.MapFrom(src => Path.Combine(currentDirectory, src.ImageQuestion))).ReverseMap(); 
        }
    }
}
