using System.Reflection;
using Xunit;

namespace DbLiteDesktop.Tests;

public class MainFormLayoutTests
{
    [Fact]
    public void HeaderTitleFitsInsideItsPanel()
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
