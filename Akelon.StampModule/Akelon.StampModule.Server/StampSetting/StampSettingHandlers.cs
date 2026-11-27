using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Akelon.StampModule.StampSetting;

namespace Akelon.StampModule
{
  partial class StampSettingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      // Установить по умолчанию, для демонстрационного решения, настройки шрифта штампа.
      _obj.StampFont = StampSetting.StampFont.Arial;
      _obj.FontSize = 18;
      _obj.FontColor = Colors.Common.Black.Name + "\\" + Colors.Common.Black.ToString();
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Валидация загруженного штампа.
      if (_obj.StampKind == StampSetting.StampKind.LoadStamp && _obj.StampImage == null)
        e.AddError(Akelon.StampModule.StampSettings.Resources.MsgNotImageStamp);
      
      // Проверка дубликатов настроек.
      if (StampSettings.GetAll().Any( s => s.Id != _obj.Id && s.Status == StampSetting.Status.Active && Equals(s.DocumentType, _obj.DocumentType)))
        e.AddError(Akelon.StampModule.StampSettings.Resources.ErrorMsgFindDuplicate);
    }
  }


}