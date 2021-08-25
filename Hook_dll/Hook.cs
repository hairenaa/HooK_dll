using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
namespace Hook_dll
{
    public class Hook
    {
        /// <summary>
        /// hook类型枚举
        /// </summary>
        public enum HookType
        {
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14

        }
        /// <summary>
        /// 鼠标事件类型类
        /// </summary>
        public class HookMouseEventType
        {

            //鼠标事件类型
            public const int WM_MOUSEMOVE = 0x200;
            public const int WM_LBUTTONDOWN = 0x201;
            public const int WM_RBUTTONDOWN = 0x204;
            public const int WM_MBUTTONDOWN = 0x207;
            public const int WM_LBUTTONUP = 0x202;
            public const int WM_RBUTTONUP = 0x205;
            public const int WM_MBUTTONUP = 0x208;
            public const int WM_LBUTTONDBLCLK = 0x203;
            public const int WM_RBUTTONDBLCLK = 0x206;
            public const int WM_MBUTTONDBLCLK = 0x209;
            //Wparam向下扩展
        }

        /// <summary>
        /// 键盘事件类型类
        /// </summary>
        public class HookKeyEventType
        {
            //键盘事件类型参数
            public const int WM_KEYDOWN = 0x0100;
            public const int WM_SYSDOWN = 0x0104;
            public const int WM_KEYUP= 0x0101;
            public const int WM_SYSKEYUP = 0x0105;
            //Wparam向下扩展
        }

        /// <summary>
        /// 鼠标事件坐标结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 鼠标事件返回结构Lparam
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MyMouseStruct
        {
            public POINT point;
            public int hWnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }


        /// <summary>
        /// 键盘事件返回结构Lparam
        /// </summary>
        public struct MyKeyStruct
        {
            public int vkcode;
            public int scancode;
            public int flags;
            public int time;
            public int dwextrainfo;





        }
        /// <summary>
        /// 鼠标事件类型列表
        /// </summary>
        public List<int> MouseEventTypeList;
        /// <summary>
        /// 键盘事件类型列表
        /// </summary>
        public List<int> KeyEventTypeList;


        #region 属性
        /// <summary>
        /// hook的类型
        /// </summary>
        public int HookTypeInt
        {
            get;
            set;
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg
        {
            get;
            private set;
        }

        //public int EventInt
        //{
        //    get;
        //    set;
        //}


        /// <summary>
        /// 最近一次的键盘事件参数结构
        /// </summary>
        public MyKeyStruct KeyLParamStruct
        {
            get;
            set;
        }
        /// <summary>
        /// 最近一次的鼠标事件参数结构
        /// </summary>
        public MyMouseStruct MouseLParamStruct
        {
            get;
            set;
        }



        /// <summary>
        /// hook句柄
        /// </summary>
        public IntPtr HookIntptr
        {
            get;
            private set;
        }

        #endregion

        #region DllImport

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookHandleDelegate lpfn, IntPtr hmod, uint dwThreadId);

        [DllImport("user32.dll")]
 
        private static extern bool UnhookWindowsHookEx(IntPtr idhook);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moudlename);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idhook, int ncode, IntPtr wparam, IntPtr lparam);


        #endregion


        
        private delegate int HookHandleDelegate(int ncode, IntPtr wparam, IntPtr lparam);
        

        private  HookHandleDelegate HookHandle;


        /// <summary>
        /// 清空键盘事件列表并设置键盘事件结构为default
        /// </summary>
        public void ClearKeyEvent()
        {
            KeyEventTypeList.Clear();
            KeyLParamStruct = default(MyKeyStruct);
        }

        /// <summary>
        /// 清空鼠标事件列表并设置鼠标事件结构为default
        /// </summary>
        public void ClearMouseEvent()
        {
            MouseEventTypeList.Clear();
            MouseLParamStruct = default(MyMouseStruct);
        }


        /// <summary>
        /// hook 回调
        /// </summary>
        /// <param name="ncode">ncode</param>
        /// <param name="wparam">事件类型</param>
        /// <param name="lparam">事件参数结构体</param>
        /// <returns></returns>
        private int HookCallback(int ncode, IntPtr wparam, IntPtr lparam)
        {
           
            if (ncode >= 0)
            {
                int wparam_int = (int)wparam;
                if (HookTypeInt == (int)HookType.WH_MOUSE_LL)
                {
                    MyMouseStruct ms = (MyMouseStruct)Marshal.PtrToStructure(lparam, typeof(MyMouseStruct));
                    MouseLParamStruct = ms;
                    if (!MouseEventTypeList.Contains(wparam_int))
                    {
                        MouseEventTypeList.Add(wparam_int);
                    }
                    
                }
                else if (HookTypeInt == (int)HookType.WH_KEYBOARD_LL)
                {
                    MyKeyStruct ks = (MyKeyStruct)Marshal.PtrToStructure(lparam, typeof(MyKeyStruct));
                    KeyLParamStruct = ks;
                    if (!KeyEventTypeList.Contains(wparam_int))
                    {
                        KeyEventTypeList.Add(wparam_int);
                    }
                }
                

                

            }
            return CallNextHookEx(HookIntptr, ncode, wparam, lparam); ;
        }

      /// <summary>
      /// 提取按键结构体的vkcode并把它转换为字符串
      /// </summary>
      /// <param name="lparamStuct"></param>
      /// <returns></returns>
        public static string GetKeyboardDownCodeStr(MyKeyStruct lparamStuct)
        {
            
            byte[] by = new byte[1];
            by[0] = (byte)(lparamStuct.vkcode);
            return Convert.ToString(Encoding.UTF8.GetString(by));
            
            

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="e">hook类型</param>
        public Hook(HookType e)
        {
            HookTypeInt = (int)e;
            MouseEventTypeList = new List<int>();
            KeyEventTypeList = new List<int>();

        }

        /// <summary>
        /// 开始钩子
        /// </summary>
        /// <param name="threadId">进程id，全局钩子不写</param>
        public void HookStart(uint threadId=0)
        {
            
            if (HookIntptr == IntPtr.Zero)
            {
                SetHook(threadId);
            }

        }




        /// <summary>
        /// 设置钩子
        /// </summary>
        /// <param name="threadId">传入的进程id</param>
        private void SetHook(uint threadId)
        {
            
            HookHandle= new HookHandleDelegate(HookCallback);
            HookIntptr = SetWindowsHookEx(HookTypeInt, HookHandle, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),threadId);
            if (HookIntptr == IntPtr.Zero)
            {
                //hookstop();
                //throw new Exception("hook安装失败");
                ErrorMsg = "hook安装失败";
            }

        }

        /// <summary>
        /// 停止钩子
        /// </summary>
        public void HookStop()
        {
            
            bool shifou_unhook = true;
            if (HookIntptr != IntPtr.Zero)
            {

                shifou_unhook = UnhookWindowsHookEx(HookIntptr);
                HookIntptr = IntPtr.Zero;
            }

            if (!shifou_unhook)
            {

                // throw new Exception("hook卸载失败");
                ErrorMsg = "hook卸载失败";

            }

        }



    }


}
