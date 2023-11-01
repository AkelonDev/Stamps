using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Akelon.StampModule.StampSetting;

namespace Akelon.StampModule.Client
{
  partial class StampSettingActions
  {
    public virtual void DeleteStampPicture(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.StampImage = null;
    }

    public virtual bool CanDeleteStampPicture(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.StampKind == StampSetting.StampKind.LoadStamp && _obj.StampImage != null;
    }

    public virtual void LoadStampPicture(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog(Akelon.StampModule.StampSettings.Resources.LoadStampDialogTheme);
      var file = dialog.AddFileSelect(Akelon.StampModule.StampSettings.Resources.FieldFileDialogLoadStamp, false);
      file.WithFilter(string.Empty, "jpg", "png");
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        _obj.StampImage = file.Value.Content;
        var sizeImage = PublicFunctions.Module.GetSizePicture(_obj.StampImage);
        _obj.SizeHeight = sizeImage[1];
        _obj.SizeWidth = sizeImage[0];
      }
    }

    public virtual bool CanLoadStampPicture(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.StampKind == StampSetting.StampKind.LoadStamp;
    }

  }

}