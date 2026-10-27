using System.Windows;
using System.Windows.Controls;

namespace TailoringShop.WPF.Views.Controls;

public partial class PaginationControl : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(1, OnPaginationPropertyChanged));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(1, OnPaginationPropertyChanged));

    public static readonly DependencyProperty TotalRecordsProperty =
        DependencyProperty.Register(nameof(TotalRecords), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(0, OnPaginationPropertyChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(10, OnPaginationPropertyChanged));

    public static readonly DependencyProperty PageChangedCommandProperty =
        DependencyProperty.Register(nameof(PageChangedCommand), typeof(System.Windows.Input.ICommand), typeof(PaginationControl),
            new PropertyMetadata(null));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int TotalRecords
    {
        get => (int)GetValue(TotalRecordsProperty);
        set => SetValue(TotalRecordsProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public System.Windows.Input.ICommand PageChangedCommand
    {
        get => (System.Windows.Input.ICommand)GetValue(PageChangedCommandProperty);
        set => SetValue(PageChangedCommandProperty, value);
    }

    #endregion

    public PaginationControl()
    {
        InitializeComponent();
        UpdateUI();
    }

    private static void OnPaginationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // تحديث نص الصفحات
        CurrentPageText.Text = CurrentPage.ToString();
        TotalPagesText.Text = TotalPages.ToString();
        TotalRecordsText.Text = TotalRecords.ToString();

        // حساب نطاق السجلات
        int startRecord = TotalRecords == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
        int endRecord = Math.Min(CurrentPage * PageSize, TotalRecords);

        StartRecordText.Text = startRecord.ToString();
        EndRecordText.Text = endRecord.ToString();

        // تحديث حالة الأزرار
        FirstButton.IsEnabled = CurrentPage > 1;
        PrevButton.IsEnabled = CurrentPage > 1;
        NextButton.IsEnabled = CurrentPage < TotalPages;
        LastButton.IsEnabled = CurrentPage < TotalPages;

        // تحديث اختيار حجم الصفحة
        SetPageSizeComboBox();
    }

    private void SetPageSizeComboBox()
    {
        // تحديد العنصر المناسب في ComboBox
        for (int i = 0; i < PageSizeComboBox.Items.Count; i++)
        {
            if (PageSizeComboBox.Items[i] is ComboBoxItem item &&
                item.Content.ToString() == PageSize.ToString())
            {
                PageSizeComboBox.SelectedIndex = i;
                break;
            }
        }
    }

    private void FirstButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            CurrentPage = 1;
            ExecutePageChanged();
        }
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            ExecutePageChanged();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            ExecutePageChanged();
        }
    }

    private void LastButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage = TotalPages;
            ExecutePageChanged();
        }
    }

    private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageSizeComboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content.ToString(), out int newPageSize))
        {
            if (newPageSize != PageSize)
            {
                PageSize = newPageSize;
                CurrentPage = 1; // العودة للصفحة الأولى عند تغيير حجم الصفحة
                ExecutePageChanged();
            }
        }
    }

    private void ExecutePageChanged()
    {
        if (PageChangedCommand?.CanExecute(null) == true)
        {
            PageChangedCommand.Execute(null);
        }
    }
}
