using System.Reflection;
using Xunit;

namespace DbLiteDesktop.Tests;

public class MainFormLayoutTests
{
    [Fact]
    public void MainToolbarTextFitsInsideRows()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new MainForm();
                form.CreateControl();
                form.PerformLayout();

                var title = GetField<Label>(form, "lblAppTitle");
                var panel = GetField<Panel>(form, "headerInfoPanel");

                Assert.True(title.Top >= 0);
                Assert.True(title.Bottom <= panel.ClientSize.Height);

                var tabMain = GetField<TabControl>(form, "tabMain");
                tabMain.SelectedTab = GetField<TabPage>(form, "tabPreview");
                form.PerformLayout();

                foreach (var name in new[]
                {
                    "btnNewConnection", "btnEditConnection", "btnDeleteConnection",
                    "btnTestConnection", "btnConnect", "btnRefresh", "btnDisconnect",
                    "cboPreviewField", "cboPreviewMatch", "txtPreviewKeyword",
                    "btnApplyPreviewFilter", "btnResetPreviewFilter"
                })
                {
                    var control = GetField<Control>(form, name);
                    Assert.True(control.Top >= 0, $"{name} 顶部超出容器");
                    Assert.True(control.Bottom <= control.Parent!.ClientSize.Height, $"{name} 底部超出容器");
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }

    private static T GetField<T>(MainForm form, string name) where T : class
    {
        return (T)typeof(MainForm)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(form)!;
    }
}
