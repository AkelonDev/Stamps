using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Akelon.StampModule.StampSetting;

namespace Akelon.StampModule
{
  partial class StampSettingSharedHandlers
  {

    public virtual void SizeWidthChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      // Установить одинаковые значения для QR-кода.
      if (e.NewValue != null && _obj.StampKind == StampSetting.StampKind.QRCode && _obj.SizeHeight != e.NewValue)
        _obj.SizeHeight = e.NewValue;
    }

    public virtual void SizeHeightChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      // Установить одинаковые значения для QR-кода.
      if (e.NewValue != null && _obj.StampKind == StampSetting.StampKind.QRCode && _obj.SizeWidth != e.NewValue)
        _obj.SizeWidth = e.NewValue;
    }

    public virtual void StampKindChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      Functions.StampSetting.SetVisibleProperties(_obj);
      Functions.StampSetting.SetRequiredProperties(_obj);
      
      // Настроить изображения штампа по умолчанию.
      if (e.NewValue == StampSetting.StampKind.Barcode && (_obj.SizeHeight != null || _obj.SizeWidth != null))
      {
        _obj.SizeHeight = null;
        _obj.SizeWidth = null;
      }
      else if (e.NewValue == StampSetting.StampKind.DrawingStamp)
      {
        _obj.SizeHeight = 20;
        _obj.SizeWidth = 125;
      }
      else if (e.NewValue == StampSetting.StampKind.QRCode)
      {
        _obj.SizeHeight = 50;
        _obj.SizeWidth = 50;
      }
      
      // Очистить загруженное изображение штампа.
      if (e.NewValue != StampSetting.StampKind.LoadStamp)
        _obj.StampImage = null;
    }
  }


}