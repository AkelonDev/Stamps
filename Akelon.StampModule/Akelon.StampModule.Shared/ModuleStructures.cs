using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Akelon.StampModule.Structures.Module
{
  /// <summary>
  /// Параметры штампа.
  /// </summary>
  [Public(Isolated = true)]
  partial class StampParameters
  {
    /// <summary>
    /// Позиция на странице.
    /// </summary>
    public string PagePosition { get; set; }
    
    /// <summary>
    /// Позиция на документе.
    /// </summary>
    public string DocumentPosition { get; set; }
    
    /// <summary>
    /// Размер штампа по ширине.
    /// </summary>
    public int? SizeWidth { get; set; }
    
    /// <summary>
    /// Размер штампа по высоте.
    /// </summary>
    public int? SizeHeight { get; set; }
    
    /// <summary>
    /// Изображение штампа.
    /// </summary>
    public byte[] StampImage { get; set; }
  }
}