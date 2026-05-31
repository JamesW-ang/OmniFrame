using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace OmniFrame
{
    /// <summary>
    /// 窗体工厂 — 从 DI 容器按需解析子窗体。
    /// 替代 MainForm 直接持有 IServiceProvider，将服务定位器隔离在此。
    /// </summary>
    public class FormFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FormFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public T GetForm<T>() where T : Form => _serviceProvider.GetService<T>();
    }
}
