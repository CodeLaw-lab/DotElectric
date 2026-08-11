using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// STA-тесты фабрик WPF-диалогов WpfDialogFileService.
/// </summary>
public class WpfDialogFileServiceTests
{
    [Fact]
    public void CreateOpenDialog_NullFilter_UsesDefaultFilter()
    {
        WpfContext.Execute(() =>
        {
            var dialog = WpfDialogFileService.CreateOpenDialog(null);

            Assert.Equal("DotElectric Template|*.tdel", dialog.Filter);
        });
    }

    [Fact]
    public void CreateOpenDialog_CustomFilter_Applied()
    {
        WpfContext.Execute(() =>
        {
            var dialog = WpfDialogFileService.CreateOpenDialog("All|*.*");

            Assert.Equal("All|*.*", dialog.Filter);
        });
    }

    [Fact]
    public void CreateOpenDialog_SetsDefaults()
    {
        WpfContext.Execute(() =>
        {
            var dialog = WpfDialogFileService.CreateOpenDialog(null);

            Assert.Equal("tdel", dialog.DefaultExt);
            Assert.False(dialog.Multiselect);
            Assert.True(dialog.CheckFileExists);
            Assert.True(dialog.CheckPathExists);
        });
    }

    [Fact]
    public void CreateSaveDialog_NullFilter_UsesDefaultFilter()
    {
        WpfContext.Execute(() =>
        {
            var dialog = WpfDialogFileService.CreateSaveDialog(null, "file.tdel");

            Assert.Equal("DotElectric Template|*.tdel|All Files|*.*", dialog.Filter);
        });
    }

    [Fact]
    public void CreateSaveDialog_CustomFilterAndFileName_Applied()
    {
        WpfContext.Execute(() =>
        {
            var dialog = WpfDialogFileService.CreateSaveDialog("XML|*.xml", "doc.xml");

            Assert.Equal("XML|*.xml", dialog.Filter);
            Assert.Equal("doc.xml", dialog.FileName);
        });
    }

    [Fact]
    public void CreateSaveDialog_SetsDefaults()
    {
        WpfContext.Execute(() =>
        {
            var dialog = WpfDialogFileService.CreateSaveDialog(null, "file.tdel");

            Assert.Equal("tdel", dialog.DefaultExt);
            Assert.True(dialog.OverwritePrompt);
            Assert.True(dialog.CheckPathExists);
        });
    }
}
