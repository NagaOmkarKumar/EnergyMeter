using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_K
{
    internal class Code
    {

        ////foreach (var thr in threads)
        //{


        //    // Trace.WriteLine(urls[0]);
        //    try
        //    {
        //        string str1 = urls[1];
        //        Thread thread = new Thread(() => FetchAndUpdateMeterData(str1))
        //        {
        //            IsBackground = true
        //        };
        //        if (!thread.IsAlive)
        //        {
        //            Trace.WriteLine(thread.ThreadState);
        //            thread.Start();
        //            threads.Add(thread);
        //        }
        //    }
        //    catch { }

        //    try
        //    {
        //        string str1 = urls[2];
        //        Thread thread2 = new Thread(() => FetchAndUpdateMeterData(str1))
        //        {
        //            IsBackground = true
        //        };
        //        if (!thread2.IsAlive)
        //        {
        //            thread2.Start();
        //            threads.Add(thread2);
        //        }
        //    }
        //    catch { }

        //    try
        //    {
        //        string str1 = urls[3];
        //        Thread thread3 = new Thread(() => FetchAndUpdateMeterData(str1))
        //        {
        //            IsBackground = true
        //        };
        //        if (!thread3.IsAlive)
        //        {
        //            thread3.Start();
        //            threads.Add(thread3);
        //        }
        //    }
        //    catch { }

        //    try
        //    {
        //        string str1 = urls[4];
        //        Thread thread4 = new Thread(() => FetchAndUpdateMeterData(str1))
        //        {
        //            IsBackground = true
        //        };
        //        if (!thread4.IsAlive)
        //        {
        //            thread4.Start();
        //            threads.Add(thread4);
        //        }
        //        Trace.WriteLine(urls[4]);
        //    }
        //    catch { }
        //    try
        //    {
        //        string str1 = urls[5];
        //        Thread thread5 = new Thread(() => FetchAndUpdateMeterData(str1))
        //        {
        //            IsBackground = true
        //        };
        //        if (!thread5.IsAlive)
        //        {
        //            thread5.Start();
        //            threads.Add(thread5);
        //        }
        //        Trace.WriteLine(urls[5]);
        //    }
        //    catch { }
        //    try
        //    {
        //        string str1 = urls[6];
        //        Thread thread6 = new Thread(() => FetchAndUpdateMeterData(str1))
        //        {
        //            IsBackground = true
        //        };
        //        if (!thread6.IsAlive)
        //        {
        //            thread6.Start();
        //            threads.Add(thread6);
        //        }
        //        Trace.WriteLine(urls[6]);
        //    }
        //    catch { }
        // }


        //private void StartMeterPolling()
        //{
        //    foreach (var thread in threads)
        //    {
        //        //Trace.WriteLine(thread.ThreadState);
        //        //Trace.WriteLine(thread.IsAlive);
        //        if (!thread.IsAlive) { try { thread.Start(); } catch { } }
        //    }
        //}

        //string volt = words.Length > 0 ? words[0].Trim() : string.Empty;
        //string curr = words.Length > 1 ? words[1].Trim() : string.Empty;
        //string pft = words.Length > 2 ? words[2].Trim() : string.Empty;

        //double voltage = 0, current = 0, pf = 0;
        //bool vol = Double.TryParse(volt, out voltage);
        //bool cur = Double.TryParse(curr, out current);
        //bool pfact = Double.TryParse(pft, out pf);

        //if (vol || cur || pfact)
        //{
        //    return;
        //}

        //double voltage1 = Convert.ToDouble(voltage);
        //double current1 = Convert.ToDouble(current);
        //double pf1 = Math.Round(Convert.ToDouble(pf), 2);

        //string kwh = words.Length >= 4 ? words[3] : "0";
        //string kva = words.Length >= 5 ? words[4] : "0";

    }
}
