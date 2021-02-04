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

        public enum HookType
        {
            wh_keyboard_ll=WH_KEYBOARD_LL,
            wh_mouse_ll=WH_MOUSE_LL,

        }

        public struct Mystruct
        {
            public int vkcode;
            public int scancode;
            public int flags;
            public int time;
            public int dwextrainfo;





        }


        #region 常量

        //键盘事件类型参数
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSDOWN = 0x0104;


        //鼠标钩子类型
        private const int WH_MOUSE_LL = 14;


        //键盘钩子类型
        private const int WH_KEYBOARD_LL = 13;

        //鼠标事件类型参数
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;

        #endregion

        #region 属性
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg
        {
            get;
            private set;
        }

        /// <summary>
        /// 钩子捕获到的数据结构Mystruct
        /// </summary>
        public Mystruct LParamStruct
        {
            get;
            private set;
        }

        /// <summary>
        /// 钩子函数的句柄
        /// </summary>
        public static IntPtr HookIntptr
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
        private static extern IntPtr CallNextHookEx(IntPtr idhook, int ncode, IntPtr wparam, IntPtr lparam);


        #endregion


        #region 委托与委托实例
        public delegate int HookHandleDelegate(int ncode, IntPtr wparam, ref Mystruct lparam);
        public delegate void MouseLeftButtonDownDelegate();
        public delegate void KeyCodeDownDelegate();
        
        private static HookHandleDelegate HookHandle;
        /// <summary>
        /// 用于响应鼠标左键按下时触发的委托实例
        /// </summary>
        public MouseLeftButtonDownDelegate MouseLeftButtonDownHandle;

        /// <summary>
        ///  用于响应键盘按键键按下时触发的委托实例
        /// </summary>
        public KeyCodeDownDelegate KeyCodeDownHandle;
        #endregion

        /// <summary>
        /// Hook类无参构造函数
        /// </summary>
        public Hook()
        {
            //GC.KeepAlive(HookHandle);//保持HookHandle不被GC回收
        }

        private int HookCallback(int ncode, IntPtr wparam, ref Mystruct lparam)
        {
            LParamStruct = lparam;
            if (ncode >= 0)
            {
                switch(wparam.ToInt32())
                {
                    case WM_KEYDOWN:
                    case WH_KEYBOARD_LL:
                        if (KeyCodeDownHandle != null)
                        {
                            KeyCodeDownHandle.Invoke();
                        }
                        break;
                    #region notes
                    //switch (lparam.vkcode)
                    //{
                    //    case 13:
                    //        str += "enter分";
                    //        return 0;

                    //    case 8:
                    //        str += "baskspace分";
                    //        return 0;
                    //    case 20:
                    //        str += "capslock分";
                    //        return 0;
                    //    case 160:
                    //        str += "shift分";
                    //        return 0;
                    //    case 161:
                    //        str += "shift分";
                    //        return 0;
                    //    case 162:
                    //        str += "ctrl分";
                    //        return 0;
                    //    case 190:
                    //        str += ".分";
                    //        return 0;
                    //}
                    #endregion
                    case WM_LBUTTONDOWN:
                        if (MouseLeftButtonDownHandle != null)
                        {
                            MouseLeftButtonDownHandle.Invoke();
                        }
                        break;
                    default:
                        break;
                }



                IntPtr newptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Mystruct)));
                Marshal.StructureToPtr(lparam, newptr, true);
                CallNextHookEx(HookIntptr, ncode, wparam, newptr);
                Marshal.FreeCoTaskMem(newptr);
                return 0;


            }
            return 0;
        }

      /// <summary>
      /// 获取按键钩子捕获的按键字符串
      /// </summary>
      /// <param name="lparamStuct">可以写LparamStruct属性</param>
      /// <returns></returns>
        public string GetKeyboardDownCodeStr(Mystruct lparamStuct)
        {
            
            byte[] by = new byte[1];
            by[0] = (byte)(lparamStuct.vkcode);
            return Convert.ToString(Encoding.UTF8.GetString(by));
            
            

        }

        /// <summary>
        /// 开始钩子参数为HookType的枚举类型 
        /// </summary>
        /// <param name="e"></param>
        public void HookStart(HookType e)
        {
            
            if (HookIntptr == IntPtr.Zero)
            {
                SetHook((int)e);
            }

        }



        private void SetHook(int id_hook,uint threadId=0)
        {

            HookHandle= new HookHandleDelegate(HookCallback);
            HookIntptr = SetWindowsHookEx(id_hook, HookHandle, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),threadId);
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
