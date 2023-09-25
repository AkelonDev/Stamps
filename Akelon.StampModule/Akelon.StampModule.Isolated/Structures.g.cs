//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Akelon.StampModule.Structures.Module
{
  public interface IStampParameters
  {
    global::System.String PagePosition { get; set; }
    global::System.String DocumentPosition { get; set; }
    global::System.Nullable<global::System.Int32> SizeWidth { get; set; }
    global::System.Nullable<global::System.Int32> SizeHeight { get; set; }
    global::System.Byte[] StampImage { get; set; }

  }

  [global::System.Serializable]
  public class StampParameters : IStampParameters
  {
    public global::System.String PagePosition { get; set; }
    public global::System.String DocumentPosition { get; set; }
    public global::System.Nullable<global::System.Int32> SizeWidth { get; set; }
    public global::System.Nullable<global::System.Int32> SizeHeight { get; set; }
    public global::System.Byte[] StampImage { get; set; }


    public static IStampParameters Create()
    {
      return new StampParameters();
    }

    public static IStampParameters Create(global::System.String pagePosition, global::System.String documentPosition, global::System.Nullable<global::System.Int32> sizeWidth, global::System.Nullable<global::System.Int32> sizeHeight, global::System.Byte[] stampImage)
    {
      return new StampParameters
      {
        PagePosition = pagePosition,
        DocumentPosition = documentPosition,
        SizeWidth = sizeWidth,
        SizeHeight = sizeHeight,
        StampImage = stampImage
      };
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((this.PagePosition != null ? this.PagePosition.GetHashCode() : 0) * 199) ^ ((this.DocumentPosition != null ? this.DocumentPosition.GetHashCode() : 0) * 199) ^ ((this.SizeWidth != null ? this.SizeWidth.GetHashCode() : 0) * 199) ^ ((this.SizeHeight != null ? this.SizeHeight.GetHashCode() : 0) * 199) ^ (this.StampImage.GetHashCode() * 199);
      }
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((StampParameters)obj);
    }

    public static bool operator ==(StampParameters left, StampParameters right)
    {
      if (ReferenceEquals(left, right))
        return true;

      if (((object)left) == null || ((object)right) == null)
        return false;

      return left.Equals(right);
    }

    public static bool operator !=(StampParameters left, StampParameters right)
    {
      return !(left == right);
    }

    protected bool Equals(StampParameters other)
    {
      return object.Equals(this.PagePosition, other.PagePosition) 
             && object.Equals(this.DocumentPosition, other.DocumentPosition) 
             && object.Equals(this.SizeWidth, other.SizeWidth) 
             && object.Equals(this.SizeHeight, other.SizeHeight) 
             && ArrayEqual(this.StampImage, other.StampImage);
    }

    private static bool ArrayEqual<TSource>(global::System.Collections.Generic.IEnumerable<TSource> left, global::System.Collections.Generic.IEnumerable<TSource> right)
    {
      if (ReferenceEquals(left, right))
        return true;
      if (ReferenceEquals(null, left))
        return false;
      if (ReferenceEquals(null, right))
        return false;
      if (global::System.Linq.Enumerable.Count(left) != global::System.Linq.Enumerable.Count(right))
        return false;

      return global::System.Linq.Enumerable.SequenceEqual(left, right);
    }
  }

}