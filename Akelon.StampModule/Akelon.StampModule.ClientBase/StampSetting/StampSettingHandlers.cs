using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Akelon.StampModule.StampSetting;

namespace Akelon.StampModule
{
  partial class StampSettingClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.StampSetting.SetVisibleProperties(_obj);
      Functions.StampSetting.SetRequiredProperties(_obj);
    }

    public virtual void SizeWidthValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(Akelon.StampModule.StampSettings.Resources.ErrorNegativeValueSize);
    }

    public virtual void SizeHeightValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(Akelon.StampModule.StampSettings.Resources.ErrorNegativeValueSize);
    }

  }
}