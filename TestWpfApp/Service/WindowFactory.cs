using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using TestWpfApp.Interfaces;

namespace TestWpfApp.Service
{


    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Create<T>() where T : Window
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }

}
