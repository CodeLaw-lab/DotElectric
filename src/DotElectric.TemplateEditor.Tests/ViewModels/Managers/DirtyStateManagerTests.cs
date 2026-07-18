using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels.Managers;

namespace DotElectric.TemplateEditor.Tests.ViewModels.Managers;

public class DirtyStateManagerTests
{
    private static Template CreateTemplate(SheetOrientation orientation = SheetOrientation.Landscape)
    {
        var sheet = Sheet.FromFormat("A4", orientation);
        var metadata = new Metadata();
        return new Template(metadata, sheet);
    }

    [Fact]
    public void Constructor_ThrowsOnNullTemplate()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new DirtyStateManager(null!));
        Assert.Equal("template", ex.ParamName);
    }

    [Fact]
    public void Constructor_InitialDefaults()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        Assert.False(sut.IsDirty);
        Assert.Null(sut.FilePath);
        Assert.Equal(string.Empty, sut.DisplayName);
    }

    [Fact]
    public void MarkDirty_SetsIsDirty()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        sut.MarkDirty();

        Assert.True(sut.IsDirty);
    }

    [Fact]
    public void MarkDirty_Idempotent_DoesNotThrow()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        sut.MarkDirty();
        sut.MarkDirty();

        Assert.True(sut.IsDirty);
    }

    [Fact]
    public void MarkDirty_TriggersPropertyChanged()
    {
        var sut = new DirtyStateManager(CreateTemplate());
        var changed = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DirtyStateManager.IsDirty))
                changed = true;
        };

        sut.MarkDirty();

        Assert.True(changed);
    }

    [Fact]
    public void ClearDirty_ClearsIsDirty()
    {
        var sut = new DirtyStateManager(CreateTemplate());
        sut.MarkDirty();
        Assert.True(sut.IsDirty);

        sut.ClearDirty();

        Assert.False(sut.IsDirty);
    }

    [Fact]
    public void ClearDirty_TriggersPropertyChanged()
    {
        var sut = new DirtyStateManager(CreateTemplate());
        sut.MarkDirty();
        var changed = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DirtyStateManager.IsDirty))
                changed = true;
        };

        sut.ClearDirty();

        Assert.True(changed);
    }

    [Fact]
    public void UpdateDisplayName_WithoutFilePath_ShowsFormatAndUntitled()
    {
        var template = CreateTemplate(SheetOrientation.Landscape);
        var sut = new DirtyStateManager(template);

        sut.UpdateDisplayName();

        Assert.Contains(template.Sheet.Format, sut.DisplayName);
        Assert.Contains("Без имени", sut.DisplayName);
    }

    [Fact]
    public void UpdateDisplayName_WithFilePath_ShowsFileName()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        sut.FilePath = @"C:\test\template.tdel";
        sut.UpdateDisplayName();

        Assert.Contains("template.tdel", sut.DisplayName);
        Assert.DoesNotContain("Без имени", sut.DisplayName);
    }

    [Theory]
    [InlineData(SheetOrientation.Portrait, "кн.")]
    [InlineData(SheetOrientation.Landscape, "алб.")]
    public void UpdateDisplayName_IncludesOrientation(SheetOrientation orientation, string expectedLabel)
    {
        var template = CreateTemplate(orientation);
        var sut = new DirtyStateManager(template);

        sut.UpdateDisplayName();

        Assert.Contains(expectedLabel, sut.DisplayName);
    }

    [Fact]
    public void FilePath_Set_StoresPath()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        sut.FilePath = @"C:\test\template.tdel";

        Assert.Equal(@"C:\test\template.tdel", sut.FilePath);
    }

    [Fact]
    public void UpdateDisplayName_AfterSettingFilePath_ShowsFileName()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        sut.FilePath = @"C:\test\template.tdel";
        sut.UpdateDisplayName();

        Assert.Contains("template.tdel", sut.DisplayName);
    }

    [Fact]
    public void DisplayName_InitiallyEmpty()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        Assert.Equal(string.Empty, sut.DisplayName);
    }

    [Fact]
    public void UpdateDisplayName_MakesDisplayNameNonEmpty()
    {
        var sut = new DirtyStateManager(CreateTemplate());

        sut.UpdateDisplayName();

        Assert.NotEmpty(sut.DisplayName);
    }

    [Fact]
    public void PropertyChanged_FiresForFilePath()
    {
        var sut = new DirtyStateManager(CreateTemplate());
        var changed = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DirtyStateManager.FilePath))
                changed = true;
        };

        sut.FilePath = @"C:\test.tdel";

        Assert.True(changed);
    }
}
