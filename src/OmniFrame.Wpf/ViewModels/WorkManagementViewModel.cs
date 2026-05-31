using System.Collections.ObjectModel;
using System.Windows.Input;
namespace OmniFrame.Wpf.ViewModels
{
    public class WorkManagementViewModel : ViewModelBase
    {
        public ObservableCollection<WorkOrderItem> Orders { get; } = new();
        private string _orderNo = "";      public string OrderNo { get => _orderNo; set => Set(ref _orderNo, value); }
        private string _productModel = ""; public string ProductModel { get => _productModel; set => Set(ref _productModel, value); }
        private int _targetQty = 100;       public int TargetQty { get => _targetQty; set => Set(ref _targetQty, value); }
        private string _status = "待机";    public string Status { get => _status; set => Set(ref _status, value); }
        private WorkOrderItem _selected;    public WorkOrderItem SelectedOrder { get => _selected; set => Set(ref _selected, value); }
        public ICommand CreateCmd { get; } public ICommand StartCmd { get; } public ICommand EndCmd { get; } public ICommand RefreshCmd { get; }
        public WorkManagementViewModel()
        {
            CreateCmd = new RelayCommand(() => { Orders.Add(new WorkOrderItem { OrderNo = OrderNo, Product = ProductModel, Qty = TargetQty, Status = "待生产" }); Status = "工单已创建"; });
            StartCmd = new RelayCommand(() => { if (SelectedOrder != null) { SelectedOrder.Status = "生产中"; Status = "生产中"; } });
            EndCmd = new RelayCommand(() => { if (SelectedOrder != null) { SelectedOrder.Status = "已完成"; Status = "待机"; } });
            RefreshCmd = new RelayCommand(() => { });
            Orders.Add(new WorkOrderItem { OrderNo = "WO-001", Product = "中框A", Qty = 500, Status = "生产中" });
            Orders.Add(new WorkOrderItem { OrderNo = "WO-002", Product = "中框B", Qty = 300, Status = "待生产" });
        }
    }
    public class WorkOrderItem { public string OrderNo { get; set; } public string Product { get; set; } public int Qty { get; set; } public string Status { get; set; } }
}
