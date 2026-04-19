// 示例1：基础初始化和轴控制
using System;
using AOIFrame.Core;

class BasicMotionExample
{
    static void Main()
    {
        try
        {
            // 初始化配置
            ConfigManager.Instance.Initialize("Config/SystemCfg.xml");
            
            // 初始化运动管理器
            MotionMgr.Instance.Initialize();
            Console.WriteLine("运动管理器初始化成功");
            
            // 设置运动参数
            MotionMgr.Instance.SetAxisVelocity(0, 100.0);
            MotionMgr.Instance.SetAxisAccel(0, 500.0);
            
            // 读取当前位置
            double currentPos = MotionMgr.Instance.GetAxisPosition(0);
            Console.WriteLine($"当前位置: {currentPos} mm");
            
            // 运动到目标位置
            double targetPos = 100.0;
            MotionMgr.Instance.MoveAbsolute(0, targetPos);
            
            // 等待运动完成
            if (MotionMgr.Instance.WaitMotionDone(0, timeout: 10000))
            {
                Console.WriteLine($"已到达目标位置: {targetPos} mm");
            }
            else
            {
                Console.WriteLine("运动超时");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }
}
