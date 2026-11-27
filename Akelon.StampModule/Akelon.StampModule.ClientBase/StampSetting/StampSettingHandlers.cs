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

    public virtual void FontSizeValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      // Проверить на минимально доступный кегль шрифта.
      if (e.NewValue < 2)
        e.NewValue = 2;
    }
    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.StampSetting.SetAvailabilityProperties(_obj);
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