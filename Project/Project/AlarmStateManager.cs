using System;
using System.Collections.Generic;
using System.Text;

namespace Project
{
    internal class AlarmStateManager
    {
        private static readonly Lazy<AlarmStateManager> _instance =
            new Lazy<AlarmStateManager>(() => new AlarmStateManager());

        public static AlarmStateManager Instance => _instance.Value;

        private AlarmStateManager() { }

        // 状态标记
        public bool IsAlarmActive { get; set; } = false;

        // 获取图片路径
        public string GetCurrentImagePath()
        {
            return IsAlarmActive
                ? "pack://application:,,,/Resources/alarm2.png"
                : "pack://application:,,,/Resources/alarm1.png";
        }

        // 切换状态
        public void ToggleAlarm()
        {
            IsAlarmActive = !IsAlarmActive;
        }
    }
}
