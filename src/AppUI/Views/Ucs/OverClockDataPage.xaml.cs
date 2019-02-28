﻿using NTMiner.Vms;
using System.Windows;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class OverClockDataPage : UserControl {
        public static void ShowWindow() {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                IconName = "Icon_OverClock",
                Width = 600,
                Height = 400,
                CloseVisible = Visibility.Visible
            }, ucFactory: (window) => new OverClockDataPage(), fixedSize: true);
        }

        public OverClockDataPageViewModel Vm {
            get {
                return (OverClockDataPageViewModel)this.DataContext;
            }
        }

        public OverClockDataPage() {
            InitializeComponent();
            ResourceDictionarySet.Instance.FillResourceDic(this, this.Resources);
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DataGrid dg = (DataGrid)sender;
            Point p = e.GetPosition(dg);
            if (p.Y < 30) {
                return;
            }
            if (dg.SelectedItem != null) {
                ((OverClockDataViewModel)dg.SelectedItem).Edit.Execute(null);
            }
        }
    }
}
