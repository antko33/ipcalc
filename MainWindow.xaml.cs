using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace IP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int NET_ADDRESS = 0;
        const int SUBNET_MASK = 1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            bool right = true;  //По умолчанию предполагаем адрес сети и маску подсети корректными
            lbxIPs.Items.Clear();
            string netAddr = txbNetAddress.Text;
            string subnetMask = txbSubnetMask.Text;
            if (!IsRight(subnetMask, SUBNET_MASK))  //Проверка маски
            {
                lbxIPs.Items.Add("Неверная маска подсети");
                right = false;
            }
            if (!IsRight(netAddr, NET_ADDRESS)) //Проверка адреса
            {
                lbxIPs.Items.Add("Неверный адрес сети");
                right = false;
            }
            if (!right) return;

            StringBuilder[] minIP, maxIP; //Первый и последний адреса хостов
            if (!CalculateIPRange(netAddr, subnetMask, out minIP, out maxIP))
            {
                lbxIPs.Items.Add("Неверная комбинация адреса и маски");
                return;
            }

            /* Преобразование minIP и maxIP в int и десятичную систему */
            int[] min = Array.ConvertAll<StringBuilder, string>(minIP, Convert.ToString).Select(n => Convert.ToInt32(n.ToString(), 2)).ToArray<int>();
            int[] max = Array.ConvertAll<StringBuilder, string>(maxIP, Convert.ToString).Select(n => Convert.ToInt32(n.ToString(), 2)).ToArray<int>();

            for (int first = min[0]; first <= max[0]; first++)
                for (int second = min[1]; second <= max[1]; second++)
                    for (int third = min[2]; third <= max[2]; third++)
                        for (int fourth = min[3]; fourth <= max[3]; fourth++)
                            lbxIPs.Items.Add(first + "." + second + "." + third + "." + fourth);    //Вывод ip из диапазона
            lbxIPs.Items.RemoveAt(0);   //Адрес сети не является хостом
            lbxIPs.Items.RemoveAt(lbxIPs.Items.Count - 1);  //Широковещательный адрес не является хостом
            lblCount.Text = lbxIPs.Items.Count.ToString();
        }

        /* Метод вычисляет диапазон адресов, принадлежащих сети. 
         Возвращает false, если вычислить не удалось,
                    true, если удалось.
         Допустимые адреса находятся между minIP и maxIP. */
        private bool CalculateIPRange(string netAddr, string subnetMask, out StringBuilder[] minIP, out StringBuilder[] maxIP)
        {
            /* Перевод адреса сети и маски в двоичную систему */
            string[] binIP = Array.ConvertAll<string, int>(netAddr.Split('.'), int.Parse).Select(n => Convert.ToString(n, 2)).ToArray<string>();
            string[] binMask = Array.ConvertAll<string, int>(subnetMask.Split('.'), int.Parse).Select(n => Convert.ToString(n, 2)).ToArray<string>();

            minIP = new StringBuilder[4];
            maxIP = new StringBuilder[4];

            for (int i = 0; i < 4; i++)
            {
                /* Дополнение ведущим нулями до 8 цифр */
                binIP[i] = binIP[i].PadLeft(8, '0');
                binMask[i] = binMask[i].PadLeft(8, '0');

                minIP[i] = new StringBuilder("");
                maxIP[i] = new StringBuilder("");

                /* Вычисление допустимых адресов:
                 * сравниваем поразрядно адрес сети и маску */
                for (int j = 0; j < 8; j++)
                {
                    if (binMask[i][j] == '1')
                    {
                        minIP[i].Append(binIP[i][j].ToString());
                        maxIP[i].Append(binIP[i][j].ToString());
                    }

                    else
                    {
                        if (binIP[i][j] == '0')
                        {
                            minIP[i].Append("0");
                            maxIP[i].Append("1");
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /* Проверка адреса сети и маски подсети на корректность.
         * Если корректно, вернёт true, иначе false */
        private bool IsRight(string obj, int objName)
        {
            string[] parts = obj.Split('.');    //Разбиение obj по точкам
            int[] iParts;
            try
            {
                iParts = Array.ConvertAll<string, int>(parts, int.Parse);
            }
            catch (Exception)
            {
                return false;   // Если не удалось преобразовать в int, obj некорректен
            }

            if (iParts[0] == 0) return false; // obj не может начинаться с 0

            try
            {
                for (int i = 0; i < 4; i++)
                {
                    if (iParts[i] > 255 || iParts[i] < 0) return false; // Числа, разделённые точками, не могут быть отрицательными
                                                                        // или превышать 255
                }
            }
            catch(Exception)
            {
                return false;
            }

            /* Проверка маски */
            if (objName == SUBNET_MASK)
            {
                string[] binParts = iParts.Select(n => Convert.ToString(n, 2)).ToArray();
                char digitControl = '1';    //Контрольная цифра. Маска неверна, если контрольная цифра равна 0,
                                            //а очередной двоичный разряд в маске - 1.
                foreach (string part in binParts)
                {
                    part.PadLeft(8, '0');   //Дополнение ведущими нулями до 8 цифр
                    foreach (char digit in part)
                    {
                        if (digitControl == '1' && digit == '0') digitControl = '0';
                        else if (digitControl == '0' && digit == '1') return false;
                    }
                }
            }
            return true;
        }
    }
}