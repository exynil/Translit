using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Translit.Models.Other;

namespace Translit.Views.DialogWindows
{
    public partial class ActivationDialogView : INotifyPropertyChanged
    {
        public SnackbarMessageQueue MessageQueue { get; set; }
        private string _key;
        private string _number;
        public bool Lock { get; set; }

        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        public string Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged();
            }
        }

        public ICommand Accept
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    if (Lock) return;

                    if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Number))
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarPleaseFillInAllFields"));
                        return;
                    }

                    MessageQueue.Enqueue(GetRes("SnackBarActivationPleaseWait"));

                    Lock = true;

                    IFirebaseClient client = new FirebaseClient(new FirebaseConfig
                    {
                        BasePath = "https://translit-10dad.firebaseio.com/Keys/",
                        AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
                    });

                    KeyInfo ki;

                    try
                    {
                        ki = client.Get(Number).ResultAs<KeyInfo>();
                    }
                    catch (Exception)
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarFailedToActivateCheckYourInternetConnection"));
                        Lock = false;
                        return;
                    }

                    if (ki == null)
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarTheKeyIsNotValid"));
                        Lock = false;
                        return;
                    }

                    if (ki.Key != Key || ki.Blocked)
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarTheKeyIsNotValid"));
                        Lock = false;
                        return;
                    }

                    if (ki.ActivatedComputers == null)
                    {
                        // Пытаемся отправить данные
                        try
                        {
                            client.Set($"{Number}/ActivatedComputers/{FingerPrint.Value()}", new ActivatedComputer
                            {
                                Id = FingerPrint.Value(),
                                ActivationDate = DateTime.Now
                            });
                        }
                        catch (Exception)
                        {
                            MessageQueue.Enqueue(GetRes("SnackBarFailedToActivateCheckYourInternetConnection"));
                            Lock = false;
                            return;
                        }

                        DialogResult = true;
                    }
                    // Если ID компьютера содержится в списке ID активированных компьютеров
                    else if (ki.ActivatedComputers.ContainsKey(FingerPrint.Value()))
                    {
                        // Принимаем активацию
                        DialogResult = true;
                    }
                    // Если активировано максимольное количество компьютеров
                    else if (ki.ActivatedComputers.Count < ki.NumberOfComputers)
                    {
                        // Пытаемся отправить данные
                        try
                        {
                            client.Set($"{Number}/ActivatedComputers/{FingerPrint.Value()}", new ActivatedComputer
                            {
                                Id = FingerPrint.Value(),
                                ActivationDate = DateTime.Now
                            });
                        }
                        catch (Exception)
                        {
                            MessageQueue.Enqueue(GetRes("SnackBarFailedToActivateCheckYourInternetConnection"));
                            Lock = false;
                            return;
                        }

                        DialogResult = true;
                    }
                    else
                    {
                        MessageQueue.Enqueue(GetRes("SnackBarMaximumNumberOfActivationsReached"));
                        Lock = false;
                    }
                });
            }
        }

        public ActivationDialogView()
        {
            InitializeComponent();
            DataContext = this;
            MessageQueue = new SnackbarMessageQueue();
            Number = "e707ec01-dfd1-4323-8ba6-17c633c1a87e";
            Key = "7e668223ba1d42fe912a513af809a0f0d1532b9ab4e6422a83a390a562bb556e1b75fe59e9d74de1906d8dc7c823e29fdebd19d02a6042c8b6bce2fb1708b03fb6dc37b7f6874ed08b8fc3e4b3b5dc7726cd495514954bcabb665dcbea37898b0866798153a34109a595202ecf5d97dd5df77e145336486f9787ccad8da21020d0d64ddae3f541f488cbce25fff081e1e5a8e7305cb34a958b9d9ccd9382dd30";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Получение ресурса по ключу
        public string GetRes(string key)
        {
            return Application.Current.Resources[key].ToString();
        }
    }
}
