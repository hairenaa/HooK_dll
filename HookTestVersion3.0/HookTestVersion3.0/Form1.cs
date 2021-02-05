using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hook_dll;
using System.Threading;

namespace HookTestVersion3._0
{
    public partial class Form1 : Form
    {
        Hook hook;
        public Form1()
        {
            InitializeComponent();
             
        }

        private void button1_Click(object sender, EventArgs e)
        {
            hook.HookStart(Hook.HookType.wh_keyboard_ll);
            ThreadPool.QueueUserWorkItem(SetValue,null);//此方法可以解决耗时操作阻碍钩子传递问题,又可解决用户在执行耗时代码时进行操作,只能返回最后一个操作值问题(对比于CallbackAsync),可以按步依次返回值,还不会阻塞UI线程.缺点是要写大量if else或switch来判断LparamStruct的值不够优雅
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hook = new Hook();
            //hook.KeyCodeDownHandle = CallBack;//会阻塞ui线程,会阻塞hook的传递
            //hook.KeyCodeDownHandle = CallBackAsync;//该方法的不足在执行耗时操作时用户相应按键或其他事件,只能是捕获到最后一次事件,因为它本质是异步,在hookcallback函数内运行,但该方法不会阻塞ui线程,且不会阻塞hook的传递

        }

        private void CallBack()
        {
            //方法运行于Hook捕获函数内部,如果是耗时操作对hook性能有所影响,加以使用多线程或是异步方法,或者读取LparamStruct的值来执行相应操作
            Thread.Sleep(5000);
            label1.Text = "Value:" + hook.GetKeyboardDownCodeStr(hook.LParamStruct);

        }

        private async void CallBackAsync()//该方法的不足在执行耗时操作时用户相应按键或其他事件,只能是捕获到最后一次事件,因为它本质是异步,在hookcallback函数内运行,但该方法不会阻塞ui线程
        {
            string txt= await Task<string>.Run(() => { Thread.Sleep(2000); return hook.GetKeyboardDownCodeStr(hook.LParamStruct); });//模拟耗时操作
            label1.Text = "Value:"+txt;
        }

        /// <summary>
        /// 第三种方法
        /// </summary>
        /// <param name="state"> 为线程池QueueUserWorkItem第二个参数传入的对象</param>
        private void SetValue(object state)//根据hook的属性LparamStruct的值来做出响应
        {
            while (true)
            {
                if (hook.LParamStruct.vkcode == 190)
                {
                    Thread.Sleep(2000);//耗时操作
                    label1.Invoke(new Action(() => { label1.Text = "Value" + "."; }));
                }
                else if(hook.LParamStruct.vkcode == 13)
                {
                    Thread.Sleep(2000);//耗时操作
                    label1.Invoke(new Action(() => { label1.Text = "Value" + "Enter"; }));
                }
            }
            
            
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            hook.HookStop();
        }

        int i = 0;
        private void button3_Click(object sender, EventArgs e)
        {
            label2.Text = "Next:" + i++;
        }
    }
}
