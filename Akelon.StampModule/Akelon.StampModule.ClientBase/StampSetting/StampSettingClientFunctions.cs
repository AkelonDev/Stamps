using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Akelon.StampModule.StampSetting;
using Akelon.StampModule.Server;

namespace Akelon.StampModule.Client
{
  partial class StampSettingFunctions
  {
    /// <summary>
    /// Установить значение цвета для шрифта.
    /// </summary>
    public void SetFontColor()
    {
      if (_obj.StampKind == StampModule.StampSetting.StampKind.TextStamp || _obj.StampKind == StampModule.StampSetting.StampKind.DrawingStamp)
      {
        // Пример выбора оттенков из основных цветов Sungero.Core.
        var delimiter = "\\";
        var colorNames = new String[]
        {
        Colors.Common.Black.Name + delimiter + Colors.Common.Black.ToString(),
        Colors.Common.Blue.Name + delimiter + Colors.Common.Blue.ToString(),
        Colors.Common.Red.Name + delimiter + Colors.Common.Red.ToString(),
        Colors.Common.Orange.Name + delimiter + Colors.Common.Orange.ToString(),
        Colors.Common.Green.Name + delimiter + Colors.Common.Green.ToString(),
        Colors.Common.Violet.Name + delimiter + Colors.Common.Violet.ToString(),
        Colors.Common.Pink.Name + delimiter + Colors.Common.Pink.ToString(),
        Colors.Common.SeaGreen.Name + delimiter + Colors.Common.SeaGreen.ToString(),
        Colors.Common.Yellow.Name + delimiter + Colors.Common.Yellow.ToString(),
        Colors.Common.Purple.Name + delimiter + Colors.Common.Purple.ToString(),
        Colors.Common.Gray.Name + delimiter + Colors.Common.Gray.ToString()
        };

        var dialog = Dialogs.CreateInputDialog(StampModule.StampSettings.Resources.ColorSelection);
        var color = dialog.AddSelect(StampModule.StampSettings.Resources.Color, true).From(colorNames);

        if (dialog.Show() == DialogButtons.Ok)
          _obj.FontColor = color.Value;
      }
      else {
        Dialogs.NotifyMessage(StampModule.StampSettings.Resources.MsgFontAvailability);
        return;
      }
    }
  }
}