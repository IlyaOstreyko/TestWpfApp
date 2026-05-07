using System.Configuration;
using System.Data;
using System.Windows;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestWpfApp.Data.Context;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Data.Repositories;
using TestWpfApp.Interfaces;
using TestWpfApp.Mappers;
using TestWpfApp.Service;
using TestWpfApp.ViewModels;
using TestWpfApp.Views;
namespace TestWpfApp
{
    public partial class App : Application
    {
        public static IHost? HostContainer { get; private set; }
        public App()
        {
            SQLitePCL.Batteries.Init();

            HostContainer = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => ConfigureServices(services))
                .Build();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(options =>
        options.UseSqlite("Data Source=TestApp.db"));
            services.AddAutoMapper(typeof(MappersQuestion));
            // Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IWindowFactory, WindowFactory>();
            services.AddTransient<MigrationOldTableService>();
            // Repositories
            services.AddTransient<IQuestionRepository, QuestionRepository>();
            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<AdminDataViewModel>();
            services.AddTransient<AdminDataWindow>();
            //services.AddTransient<AddQuestionViewModel>();
            //services.AddTransient<EditQuestionsViewModel>();
            //services.AddTransient<ResaultsViewModel>();
            //services.AddTransient<ShowQuestionViewModel>();
            //services.AddTransient<ShowThemesViewModel>();
            //services.AddTransient<UserViewModel>();
            // Windows
            services.AddTransient<MainWindow>();
            services.AddTransient<AddQuestion>();
            services.AddTransient<EditQuestions>();
            services.AddTransient<NewTheme>();
            services.AddTransient<Resaults>();
            services.AddTransient<Sample>();
            services.AddTransient<ShowQuestion>();
            services.AddTransient<ShowThemes>();
            services.AddTransient<User>();
        }

        protected override async void OnStartup(StartupEventArgs e) 
        {
            await HostContainer!.StartAsync();

            // Получаем окно из контейнера — DI сам пробросит в него ViewModel
            var window = HostContainer.Services.GetRequiredService<MainWindow>();
            window.Show();

            base.OnStartup(e);
        } 
        protected override async void OnExit(ExitEventArgs e)
        {
            using (HostContainer)
            {
                await HostContainer!.StopAsync();
            }
            base.OnExit(e);
        }            
    }
}
//public App()
//{
//    HostContainer = Host.CreateDefaultBuilder().ConfigureServices((context, services) => { 
//        // AutoMapper
//        services.AddAutoMapper(typeof(MappersQuestion)); 
//        // Services
//        services.AddSingleton<IDialogService, DialogService>(); 
//        services.AddSingleton<IWindowService, WindowService>();
//        services.AddSingleton<IWindowFactory, WindowFactory>();
//        // Repositories
//        services.AddTransient<IQuestionRepository, QuestionRepository>();
//        // ViewModels
//        services.AddTransient<MainWindowViewModel>();
//        services.AddTransient<AddQuestionViewModel>();
//        services.AddTransient<EditQuestionsViewModel>();
//        services.AddTransient<ResaultsViewModel>();
//        services.AddTransient<ShowQuestionViewModel>();
//        services.AddTransient<ShowThemesViewModel>();
//        services.AddTransient<UserViewModel>();
//        // Windows
//        services.AddTransient<MainWindow>();
//        services.AddTransient<AddQuestion>();
//        services.AddTransient<EditQuestions>();
//        services.AddTransient<NewTheme>();
//        services.AddTransient<Resaults>();
//        services.AddTransient<Sample>();
//        services.AddTransient<ShowQuestion>();
//        services.AddTransient<ShowThemes>();
//        services.AddTransient<User>();
//    }) .Build(); 
//} 