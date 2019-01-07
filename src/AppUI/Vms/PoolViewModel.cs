﻿using NTMiner.Core;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class PoolViewModel : ViewModelBase, IPool {
        public static readonly PoolViewModel PleaseSelect = new PoolViewModel(Guid.Empty) {
            _coinId = Guid.Empty,
            _name = "请选择"
        };
        private DataLevel _dataLevel = DataLevel.UnDefined;
        private Guid _id;
        private string _name;
        private Guid _coinId;
        private string _server;
        private string _url;
        private int _sortNumber;
        private string _description;
        private PublishStatus _publishState;
        private bool _isCurrentPool;

        public Guid GetId() {
            return this.Id;
        }

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand SortUp { get; private set; }
        public ICommand SortDown { get; private set; }

        public ICommand ViewPoolIncome { get; private set; }
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public PoolViewModel() {
            if (!Design.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public PoolViewModel(IPool data) : this(data.GetId()) {
            _dataLevel = data.DataLevel;
            _name = data.Name;
            _coinId = data.CoinId;
            _server = data.Server;
            _url = data.Url;
            _sortNumber = data.SortNumber;
            _description = data.Description;
            _publishState = data.PublishState;
        }

        public PoolViewModel(Guid id) {
            _id = id;
            this.Save = new DelegateCommand(() => {
                if (NTMinerRoot.Current.PoolSet.Contains(this.Id)) {
                    Global.Execute(new UpdatePoolCommand(this));
                }
                else {
                    Global.Execute(new AddPoolCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand(() => {
                PoolEdit.ShowEditWindow(this);
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                DialogWindow.ShowDialog(message: $"您确定删除{this.Name}矿池吗？", title: "确认", onYes: () => {
                    Global.Execute(new RemovePoolCommand(this.Id));
                }, icon: "Icon_Confirm");
            });
            this.SortUp = new DelegateCommand(() => {
                PoolViewModel upOne = PoolViewModels.Current.AllPools.OrderByDescending(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber < this.SortNumber);
                if (upOne != null) {
                    int sortNumber = upOne.SortNumber;
                    upOne.SortNumber = this.SortNumber;
                    Global.Execute(new UpdatePoolCommand(upOne));
                    this.SortNumber = sortNumber;
                    Global.Execute(new UpdatePoolCommand(this));
                    if (CoinViewModels.Current.TryGetCoinVm(this.CoinId, out CoinViewModel coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.Pools));
                        coinVm.OnPropertyChanged(nameof(coinVm.OptionPools));
                    }
                }
            });
            this.SortDown = new DelegateCommand(() => {
                PoolViewModel nextOne = PoolViewModels.Current.AllPools.OrderBy(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber > this.SortNumber);
                if (nextOne != null) {
                    int sortNumber = nextOne.SortNumber;
                    nextOne.SortNumber = this.SortNumber;
                    Global.Execute(new UpdatePoolCommand(nextOne));
                    this.SortNumber = sortNumber;
                    Global.Execute(new UpdatePoolCommand(this));
                    if (CoinViewModels.Current.TryGetCoinVm(this.CoinId, out CoinViewModel coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.Pools));
                        coinVm.OnPropertyChanged(nameof(coinVm.OptionPools));
                    }
                }
            });
            this.ViewPoolIncome = new DelegateCommand<WalletViewModel>((wallet) => {
                Process.Start(this.Url.Replace("{wallet}", wallet.Address));
            });
        }

        public bool IsCurrentPool {
            get { return _isCurrentPool; }
            set {
                _isCurrentPool = value;
                OnPropertyChanged(nameof(IsCurrentPool));
            }
        }

        public DataLevel DataLevel {
            get { return _dataLevel; }
            set {
                _dataLevel = value;
                OnPropertyChanged(nameof(DataLevel));
                OnPropertyChanged(nameof(DataLevelText));
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }

        public bool IsReadOnly {
            get {
                if (!DevMode.IsDevMode && this.DataLevel == DataLevel.Global) {
                    return true;
                }
                return false;
            }
        }

        public string DataLevelText {
            get {
                return this.DataLevel.GetDescription();
            }
        }

        public void SetDataLevel(DataLevel dataLevel) {
            this.DataLevel = dataLevel;
        }

        public Guid Id {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                    if (this == PleaseSelect) {
                        return;
                    }
                    if (string.IsNullOrEmpty(value)) {
                        throw new ValidationException("名称是必须的");
                    }
                    if (PoolViewModels.Current.AllPools.Any(a => a.Name == value && a.Id != this.Id)) {
                        throw new ValidationException("名称重复");
                    }
                }
            }
        }

        public Guid CoinId {
            get {
                return _coinId;
            }
            set {
                _coinId = value;
                OnPropertyChanged(nameof(CoinId));
            }
        }

        public string CoinCode {
            get {
                if (NTMinerRoot.Current.CoinSet.TryGetCoin(this.CoinId, out ICoin coin)) {
                    return coin.Code;
                }
                return string.Empty;
            }
        }

        public string Server {
            get => _server;
            set {
                if (_server != value) {
                    _server = value;
                    OnPropertyChanged(nameof(Server));
                    if (string.IsNullOrEmpty(value)) {
                        throw new ValidationException("矿池地址是必须的");
                    }
                }
            }
        }

        public string Url {
            get => _url;
            set {
                _url = value;
                OnPropertyChanged(nameof(Url));
            }
        }

        public int SortNumber {
            get => _sortNumber;
            set {
                _sortNumber = value;
                OnPropertyChanged(nameof(SortNumber));
            }
        }

        public PublishStatus PublishState {
            get => _publishState;
            set {
                _publishState = value;
                OnPropertyChanged(nameof(PublishState));
            }
        }

        public string Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }
}
