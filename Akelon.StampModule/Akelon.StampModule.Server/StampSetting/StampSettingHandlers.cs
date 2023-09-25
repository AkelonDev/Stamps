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

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (_obj.StampKind == StampSetting.StampKind.LoadStamp && _obj.StampImage == null)
        e.AddError(Akelon.StampModule.StampSettings.Resources.MsgNotImageStamp);
      
      if (StampSettings.GetAll().Any( s => s.Id != _obj.Id && s.Status == StampSetting.Status.Active && Equals(s.DocumentType, _obj.DocumentType)))
        e.AddError(Akelon.StampModule.StampSettings.Resources.ErrorMsgFindDuplicate);
    }
  }


}