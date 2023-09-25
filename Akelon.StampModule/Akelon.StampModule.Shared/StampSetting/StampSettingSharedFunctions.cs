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
    /// Установить видимость свойств в зависимости от заполненных данных.
    /// </summary>
    public virtual void SetVisibleProperties()
    {
      var properties = _obj.State.Properties;
      
      properties.StampImage.IsVisible = _obj.StampKind == StampModule.StampSetting.StampKind.LoadStamp;
      
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