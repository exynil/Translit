using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Net;

namespace TranslitKeyGen
{
    class Program
    {
        static void Main()
        {
            IFirebaseClient client = new FirebaseClient(new FirebaseConfig
            {
                BasePath = "https://translit-10dad.firebaseio.com/",
                AuthSecret = "1V3A4v70wJZeZuvh4VwDmeV562zDSjuF4qDnrqtF"
            });

            Console.Write("Длина ключа: ");
            var length = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Кол-во компьютеров: ");
            var numberOfComputers = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Владелец: ");
            var owner = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Заметка: ");
            var note = Console.ReadLine() ?? throw new InvalidOperationException();

            var key = "";

            for (var i = 0; i < length; i++)
            {
                key += Guid.NewGuid().ToString().Replace("-", "");
            }

            SetResponse response = null;

            var number = Guid.NewGuid();

            try
            {
                response = client.Set($"Keys/{number}", new KeyInfo
                {
                    Key = key,
                    NumberOfComputers = numberOfComputers,
                    Owner = owner,
                    Note = note
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("\nВаш номер:\n" + number);

                Console.WriteLine("\nВаш ключ:\n" + key);

                Console.WriteLine("\nГотово!");
            }
            else
            {
                Console.WriteLine("\nЧто-то пошло не так...");
                if (response != null) Console.WriteLine($"\n{response.StatusCode}");
            }

            Console.ReadLine();
        }
    }
}
