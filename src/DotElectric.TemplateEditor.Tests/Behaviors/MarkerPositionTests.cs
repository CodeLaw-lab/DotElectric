using System.Windows;
using DotElectric.TemplateEditor.Behaviors;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class MarkerPositionTests
{
    // ===== XPropertyPath DP =====

    [Fact]
    public void SetXPropertyPath_OnDependencyObject_SetsValue()
    {
        var obj = new DependencyObject();
        MarkerPosition.SetXPropertyPath(obj, "MicronsX");
        var result = MarkerPosition.GetXPropertyPath(obj);
        Assert.Equal("MicronsX", result);
    }

    [Fact]
    public void GetXPropertyPath_Default_ReturnsNull()
    {
        var obj = new DependencyObject();
        var result = MarkerPosition.GetXPropertyPath(obj);
        Assert.Null(result);
    }

    [Fact]
    public void SetXPropertyPath_ToNull_ClearsValue()
    {
        var obj = new DependencyObject();
        MarkerPosition.SetXPropertyPath(obj, "MicronsX");
        MarkerPosition.SetXPropertyPath(obj, null);
        Assert.Null(MarkerPosition.GetXPropertyPath(obj));
    }

    [Fact]
    public void SetXPropertyPath_EmptyString_StoredAsEmpty()
    {
        var obj = new DependencyObject();
        MarkerPosition.SetXPropertyPath(obj, "");
        Assert.Equal("", MarkerPosition.GetXPropertyPath(obj));
    }

    // ===== YPropertyPath DP =====

    [Fact]
    public void SetYPropertyPath_OnDependencyObject_SetsValue()
    {
        var obj = new DependencyObject();
        MarkerPosition.SetYPropertyPath(obj, "BottomMicronsY");
        var result = MarkerPosition.GetYPropertyPath(obj);
        Assert.Equal("BottomMicronsY", result);
    }

    [Fact]
    public void GetYPropertyPath_Default_ReturnsNull()
    {
        var obj = new DependencyObject();
        var result = MarkerPosition.GetYPropertyPath(obj);
        Assert.Null(result);
    }

    [Fact]
    public void SetYPropertyPath_ToNull_ClearsValue()
    {
        var obj = new DependencyObject();
        MarkerPosition.SetYPropertyPath(obj, "BottomMicronsY");
        MarkerPosition.SetYPropertyPath(obj, null);
        Assert.Null(MarkerPosition.GetYPropertyPath(obj));
    }

    [Fact]
    public void SetYPropertyPath_EmptyString_StoredAsEmpty()
    {
        var obj = new DependencyObject();
        MarkerPosition.SetYPropertyPath(obj, "");
        Assert.Equal("", MarkerPosition.GetYPropertyPath(obj));
    }

    // ===== Independent DPs =====

    [Fact]
    public void XAndYPropertyPaths_AreIndependent()
    {
        var obj = new DependencyObject();
        MarkerPosition.SetXPropertyPath(obj, "MicronsX");
        MarkerPosition.SetYPropertyPath(obj, "BottomMicronsY");

        Assert.Equal("MicronsX", MarkerPosition.GetXPropertyPath(obj));
        Assert.Equal("BottomMicronsY", MarkerPosition.GetYPropertyPath(obj));
    }

    [Fact]
    public void DifferentObjects_HaveDifferentValues()
    {
        var obj1 = new DependencyObject();
        var obj2 = new DependencyObject();
        MarkerPosition.SetXPropertyPath(obj1, "A");
        MarkerPosition.SetXPropertyPath(obj2, "B");

        Assert.Equal("A", MarkerPosition.GetXPropertyPath(obj1));
        Assert.Equal("B", MarkerPosition.GetXPropertyPath(obj2));
    }
}
