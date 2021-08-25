# HooK_dll
You can use this hook_ Dll.dll class library file to set the hook to intercept keyboard or mouse events
# how to use it
Add a DLL to the project reference and use the **using** statement to reference it
## Use code example:

```
using Hook_dll;

private void timer1_Tick(object sender, EventArgs e)
        {
            

            if (mousehook.MouseEventTypeList.Contains(Hook.HookMouseEventType.WM_RBUTTONDOWN))
            {
               //your codes

                mousehook.ClearMouseEvent();
            }

            if (keyhook.KeyEventTypeList.Contains( Hook.HookKeyEventType.WM_KEYDOWN))
            {
                if (Hook.GetKeyboardDownCodeStr(keyhook.KeyLParamStruct).ToUpper() == "N")
                {
                   
                    //your codes
                }
                keyhook.ClearKeyEvent();

            }
      }  

