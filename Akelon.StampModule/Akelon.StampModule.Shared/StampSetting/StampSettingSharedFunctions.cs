using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Akelon.StampModule.StampSetting;

namespace Akelon.StampModule.Shared
{
  partial class StampSettingFunctions
  {
    /// <summary>
    /// Установить доступность свойств в зависимости от заполненных данных.
    /// </summary>
    public virtual void SetAvailabilityProperties()
    {
      var properties = _obj.State.Properties;
      
      properties.StampImage.IsVisible = _obj.StampKind == StampModule.StampSetting.StampKind.LoadStamp;
      
      var enableFont = _obj.StampKind == StampModule.StampSetting.StampKind.TextStamp || _obj.StampKind == StampModule.StampSetting.StampKind.DrawingStamp;
      properties.StampFont.IsEnabled = enableFont;
      properties.FontSize.IsEnabled = enableFont;
      properties.FontColor.IsEnabled = enableFont;

      var isSizeVisible = _obj.StampKind != StampModule.StampSetting.StampKind.Barcode;
      properties.SizeHeight.IsVisible = isSizeVisible;
      properties.SizeWidth.IsVisible = isSizeVisible;       
    }
    
    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public virtual void SetRequiredProperties()
    {      
      var properties = _obj.State.Properties;
      
      var isSizeRequired = _obj.StampKind != StampModule.StampSetting.StampKind.Barcode;
      properties.SizeHeight.IsRequired = isSizeRequired;
      properties.SizeWidth.IsRequired = isSizeRequired;       
    }
  }
}