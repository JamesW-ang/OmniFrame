// 示例2：IO控制和事件处理
using System;
using AOIFrame.Core;

class IOControlExample
{
    static void Main()
    {
        try
        {
            // 初始化IO管理器
            IoMgr.Instance.Initialize();
            
            // 订阅DI变化事件
            IoMgr.Instance.OnDIChanged += (index, state) =>
            {
                Console.WriteLine($"[事件] DI{index} 变化为 {state}");
                
                if (index == 0 && state) // 如果DI0变为高
                {
                    Console.WriteLine("检测到启动信号");
                    IoMgr.Instance.WriteDO(0, true); // 启动电机
                }
            };
            
            // 订阅DO变化事件
            IoMgr.Instance.OnDOChanged += (index, state) =>
            {
                Console.WriteLine($"[事件] DO{index} 设置为 {state}");
            };
            
            // 定时检查IO状态
            for (int i = 0; i < 10; i++)
            {
                // 读取DI
                bool di0 = IoMgr.Instance.ReadDI(0);
                bool di1 = IoMgr.Instance.ReadDI(1);
                Console.WriteLine($"DI状态: DI0={di0}, DI1={di1}");
                
                // 读取模拟输入
                double ai0 = IoMgr.Instance.ReadAI(0);
                Console.WriteLine($"AI0值: {ai0} V");
                
                // 设置模拟输出
                IoMgr.Instance.WriteAO(0, 5.0 * (i / 10.0)); // 0-5V渐变
                
                System.Threading.Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }
}
