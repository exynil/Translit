using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using MaterialDesignThemes.Wpf;
using Translit.Models.Other;

namespace Translit.Views.DialogWindows
{
    public partial class ActivationDialogView : INotifyPropertyChanged
    {
        private string _key;
        private string _number;

        public ActivationDialogView()
        {
            InitializeComponent();
            DataContext = this;
            MessageQueue = new SnackbarMessageQueue();
        }

        public SnackbarMessageQueue MessageQueue { get; set; }
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