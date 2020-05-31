using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
public class FSP : MonoBehaviour
{
    public class Task
    {
        public int N;
        public int[] P;   // Czas wykonania
        public Task Next;
        public Task Prev;
        public Task(int[] p, int i)
        {
            N = i;
            P = (int[])p.Clone();
        }
        public int Forget()
        {
            Next.Prev = Prev;
            Prev.Next = Next;
            return Next.N;
        }
        public void Remind()
        {
            Next.Prev = this;
            Prev.Next = this;
        }
    }

    List<Task> Tasks;
    int TasksCount, DevicesCount;
    int[] Permutacja, NajlepszaPerm;
    int[,] Zakonczenia;
    float NajlepszyCzas;
    void Start()
    {
        string[] path = {
            "Assets/NEH/ta115.txt" ,
            "Assets/NEH/ta116.txt" ,
            "Assets/NEH/ta117.txt" ,  // [2] ze 3 minuty
            "Assets/NEH/ta118.txt" ,
            "Assets/NEH/ta119.txt" ,
            "Assets/NEH/ta120.txt"
        };
        for (int a = 0; a < path.Length; a++)
        //for (int a = 0; a < 4; a++)
        {
            //int a = 3;
            Tasks = new List<Task>();
            StreamReader Reader = new StreamReader(path[a]);
            var Content = Reader.ReadToEnd();
            Reader.Close();
            //UnityEngine.Debug.Log(Content);
            string[] LineSeparators = new string[] { "\r\n", "\n", "\r" };
            string[] Lines = Content.Split(LineSeparators, 0);
            string[] stringSeparators = new string[] { " " };
            string[] FirstLine = Lines[1].Split(stringSeparators, 0);
            //UnityEngine.Debug.Log("_"+Lines[0]+"_");
            TasksCount = int.Parse(FirstLine[0]);
            DevicesCount = int.Parse(FirstLine[1]);
            NajlepszaPerm = new int[TasksCount];
            Permutacja = new int[TasksCount];
            Zakonczenia = new int[TasksCount, DevicesCount];
            OstIteracja = TasksCount - 2;
            //UnityEngine.Debug.Log(TasksCount);
            //UnityEngine.Debug.Log(DevicesCount);
            for (int i = 0; i < TasksCount; i++)
            {
                //UnityEngine.Debug.Log(Lines[i]);
                string[] Line = Lines[i + 2].Split(stringSeparators, 0);
                int[] P = new int[DevicesCount];
                for (int b = 0, c = 0, d = 0; b < Line.Length; b++)
                {
                    if (Line[b] == "") continue;
                    if (c % 2 == 1)
                    {
                        P[d] = int.Parse(Line[b]);
                        d++;
                    }
                    c++;
                }
                Task task = new Task(P, i);
                Permutacja[i] = i;
                Tasks.Add(task);

            }

            for (int i = 0; i < TasksCount - 1; i++)
            {
                Tasks[i].Next = Tasks[i + 1];
            }
            Tasks[TasksCount - 1].Next = Tasks[0];
            for (int i = 1; i < TasksCount; i++)
            {
                Tasks[i].Prev = Tasks[i - 1];
            }
            Tasks[0].Prev = Tasks[TasksCount - 1];

            // Inicjalizacja
            NajlepszyCzas = Mathf.Infinity;
            Iteracja = 0;
            OstIteracja = TasksCount - 2;

            //UnityEngine.Debug.Log("Czas: " + PoliczPermutacje().ToString());

            //BruteForce(0);
            //UnityEngine.Debug.Log("Najlepszy Czas: " + NajlepszyCzas.ToString());
            Stopwatch stopwatch = new Stopwatch();
            UnityEngine.Debug.Log("Plik: " + a);

            init();
            stopwatch.Start();
            int res = Johnson();
            stopwatch.Stop();
            UnityEngine.Debug.Log("Czas: " + res.ToString() + "Time: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();

            stopwatch.Start();
            res = NEH();
            stopwatch.Stop();
            UnityEngine.Debug.Log("Czas: " + res.ToString() + "Time: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();

            stopwatch.Start();
            res = NEH2();
            stopwatch.Stop();
            UnityEngine.Debug.Log("Czas: " + res.ToString() + "Time: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();

            stopwatch.Start();
            res = NEH3();
            stopwatch.Stop();
            UnityEngine.Debug.Log("Czas: " + res.ToString() + "Time: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            /*
            init();
            stopwatch.Start();
            BnB1(0);
            stopwatch.Stop();
            UnityEngine.Debug.Log("BnB1: " + NajlepszyCzas.ToString() + "Time: " + stopwatch.ElapsedMilliseconds + " Rekurencji: " + Rekurencje);
            stopwatch.Reset();

            init();
            stopwatch.Start();
            BnB3(0);
            stopwatch.Stop();
            UnityEngine.Debug.Log("BnB3: " + NajlepszyCzas.ToString() + "Time: " + stopwatch.ElapsedMilliseconds + " Rekurencji: " + Rekurencje);
            stopwatch.Reset();

            init();
            stopwatch.Start();
            BnB4(0);
            stopwatch.Stop();
            UnityEngine.Debug.Log("BnB4: " + NajlepszyCzas.ToString() + "Time: " + stopwatch.ElapsedMilliseconds + " Rekurencji: " + Rekurencje);
            stopwatch.Reset();

            init();
            stopwatch.Start();
            BruteForce(0);
            stopwatch.Stop();
            UnityEngine.Debug.Log("Brute: " + NajlepszyCzas.ToString() + "Time: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            */
            UnityEngine.Debug.Log("-------------");
        }
        void init()
        {
            NajlepszyCzas = Mathf.Infinity;
            Iteracja = 0;
            OstIteracja = TasksCount - 2;
            Rekurencje = 0;
            TrzeciCzlon = new int[DevicesCount];
        }
    }
    int Rekurencje;
    int NEH3()
    {
        float Cmin, c;
        int[] Perm;
        int ZadanieII = 0, zII, zI = 0;
        Task task;
        List<Task> tasks = new List<Task>(Tasks);
        tasks.Sort(
            delegate (Task t1, Task t2)
            {
                int a;
                int p1 = 0;
                for (a = 0; a < t1.P.Length; a++)
                {
                    p1 += t1.P[a];
                }
                int p2 = 0;
                for (a = 0; a < t2.P.Length; a++)
                {
                    p2 += t2.P[a];
                }
                int res = p1.CompareTo(p2);

                return res;
            }
        );
        Permutacja = new int[TasksCount];
        Perm = new int[TasksCount];
        Permutacja[0] = tasks[0].N;
        // Dla wszytskich pozostałych zadan 
        for (int a = 1; a < TasksCount; a++)
        {
            task = tasks[a];
            Cmin = Mathf.Infinity;
            // Wszystke mozliwossci wstawienia
            for (int b = 0; b <= a; b++)
            {
                zI = task.N;
                Wstaw(b, zI);
                c = PoliczPermutacjeDo(a);
                if (c < Cmin)
                {
                    Cmin = c;
                    Array.Copy(Permutacja, Perm, a + 1);
                    
                }
                Napraw(b);

            }
            Array.Copy(Perm, Permutacja, a + 1);
            // Po wstawieniu zadania etap II

            Cmin = Mathf.Infinity;
            for (int b = 0; b <= a; b++)
            {
                if (Permutacja[b] == zI) continue;  // Jak natrafimy na zadanie ktore przydzielalismy
                zII = Permutacja[b];
                Napraw(b);
                c = PoliczPermutacjeDo(a - 1);
                if (c < Cmin)
                {
                    // Usuniecie tego sprawi najwieksza poprawe
                    ZadanieII = zII;
                    Cmin = c;
                }
                Wstaw(b, zII);
            }

            task = Tasks[ZadanieII];
            UsunZadanie();
            Cmin = Mathf.Infinity;
            for (int b = 0; b <= a; b++)
            {
                Wstaw(b, task.N);
                c = PoliczPermutacjeDo(a);
                if (c < Cmin)
                {
                    Cmin = c;
                    Array.Copy(Permutacja, Perm, a + 1);
                }
                Napraw(b);
            }
            Array.Copy(Perm, Permutacja, a + 1);
        }
        return PoliczPermutacje();

        void Wstaw(int num, int val)
        {
            for (int a = TasksCount - 1; a > num; a--)
            {
                Permutacja[a] = Permutacja[a - 1];
            }
            Permutacja[num] = val;
        }

        void Napraw(int num)
        {
            for (int a = num; a < TasksCount - 1; a++)
            {
                Permutacja[a] = Permutacja[a + 1];
            }
        }

        void UsunZadanie()
        {
            int num = 0;
            // Sprawdzenie gdzie lezy zadanie o numerze II
            for (int a = 0; a < TasksCount - 1; a++)
            {
                if (Permutacja[a] == ZadanieII)
                {
                    num = a;
                    break;
                }
            }
            for (int a = num; a < TasksCount - 1; a++)
            {
                Permutacja[a] = Permutacja[a + 1];
            }
        }
    }
    int NEH2() {
        float Cmin, c;
        int[] Perm;
        int ZadanieII, zI = 0;
        Task task;
        List<Task> tasks = new List<Task>(Tasks);
        tasks.Sort(
            delegate (Task t1, Task t2)
            {
                int a;
                int p1 = 0;
                for (a = 0; a < t1.P.Length; a++)
                {
                    p1 += t1.P[a];
                }
                int p2 = 0;
                for (a = 0; a < t2.P.Length; a++)
                {
                    p2 += t2.P[a];
                }
                int res = p1.CompareTo(p2);

                return res;
            }
        );
        Permutacja = new int[TasksCount];
        Perm = new int[TasksCount];
        Permutacja[0] = tasks[0].N;
        // Dla wszytskich pozostałych zadan 
        for (int a = 1; a < TasksCount; a++)
        {
            task = tasks[a];
            Cmin = Mathf.Infinity;
            // Wszystke mozliwossci wstawienia
            for (int b = 0; b <= a; b++)
            {
                zI = task.N;
                Wstaw(b, zI);
                c = PoliczPermutacjeDo(a);
                if (c < Cmin)
                {
                    Cmin = c;
                    Array.Copy(Permutacja, Perm, a + 1);
                }
                Napraw(b);
                
            }
            Array.Copy(Perm, Permutacja, a + 1);
            // Po wstawieniu zadania etap II
            Cmin = Mathf.Infinity;
            ZadanieII = NumerZadaniaZKryterium(a, zI);
            UsunZadanie();
            task = Tasks[ZadanieII];
            for (int b = 0; b <= a; b++)
            {
                Wstaw(b, task.N);
                c = PoliczPermutacjeDo(a);
                if (c < Cmin)
                {
                    Cmin = c;
                    Array.Copy(Permutacja, Perm, a + 1);
                }
                Napraw(b);
            }
            Array.Copy(Perm, Permutacja, a + 1);
        }
        return PoliczPermutacje();

        void Wstaw(int num, int val)
        {
            for (int a = TasksCount - 1; a > num; a--)
            {
                Permutacja[a] = Permutacja[a - 1];
            }
            Permutacja[num] = val;
        }

        void Napraw(int num)
        {
            for (int a = num; a < TasksCount - 1; a++)
            {
                Permutacja[a] = Permutacja[a + 1];
            }
        }

        void UsunZadanie()
        {
            int num = 0;
            // Sprawdzenie gdzie lezy zadanie o numerze II
            for (int a = 0; a < TasksCount - 1; a++)
            {
                if (Permutacja[a] == ZadanieII)
                {
                    num = a;
                    break;
                }
            }
            for (int a = num; a < TasksCount - 1; a++)
            {
                Permutacja[a] = Permutacja[a + 1];
            }
        }
    }
    int NEH()
    {
        float Cmin, c;
        int[] Perm;
        List<Task> tasks = new List<Task>(Tasks);
        tasks.Sort(
            delegate (Task t1, Task t2)
            {
                int a;
                int p1 = 0;
                for (a = 0; a < t1.P.Length; a++)
                {
                    p1 += t1.P[a];
                }
                int p2 = 0;
                for (a = 0; a < t2.P.Length; a++)
                {
                    p2 += t2.P[a];
                }
                int res = p1.CompareTo(p2);

                return res;
            }
        );
        Permutacja = new int[TasksCount];
        Perm = new int[TasksCount];
        Permutacja[0] = tasks[0].N;
        // Dla wszytskich pozostałych zadan 
        for (int a = 1; a < TasksCount; a++)
        {
            Task task = tasks[a];
            Cmin = Mathf.Infinity;
            // Wszystke mozliwossci wstawienia
            for (int b = 0; b <= a; b++)
            {
                Wstaw(b, task.N);
                c = PoliczPermutacjeDo(a);
                if (c < Cmin)
                {
                    Cmin = c;
                    Array.Copy(Permutacja, Perm, a + 1);
                }
                Napraw(b);
            }
            Array.Copy(Perm, Permutacja, a + 1);
        }
        return PoliczPermutacje();

        void Wstaw(int num, int val)
        {
            for (int a = TasksCount - 1; a > num; a--)
            {
                Permutacja[a] = Permutacja[a - 1];
            }
            Permutacja[num] = val;
        }

        void Napraw(int num)
        {
            for (int a = num; a < TasksCount - 1; a++)
            {
                Permutacja[a] = Permutacja[a + 1];
            }
        }
    }
    void BnB1(int Pierwszy)
    {
        Task Zadanie;
        int LB;
        for (int i = TasksCount - Iteracja; i > 0; i--)
        {
            Permutacja[Iteracja] = Tasks[Pierwszy].N;
            if (Iteracja != OstIteracja)
            {
                Zadanie = Tasks[Pierwszy];
                Pierwszy = Zadanie.Forget();    // Funkcja zwraca numer nastepnego zadania
                Iteracja++;
                LB = Bound1(Pierwszy);
                if (LB <= NajlepszyCzas)
                {
                    Rekurencje++;
                    BnB1(Pierwszy);
                }
                Iteracja--;
                Zadanie.Remind();
            }
            else
            {
                Pierwszy = Tasks[Pierwszy].Next.N;
                Permutacja[Iteracja + 1] = Tasks[Pierwszy].N;
                PoliczPermutacje(); // Funkcja załatwia sprawe z uaktualnieniem UB i zapisania permutacji
               
            }
        }
    }

    void BnB3(int Pierwszy)
    {
        Task Zadanie;
        int LB;
        for (int i = TasksCount - Iteracja; i > 0; i--)
        {
            Permutacja[Iteracja] = Tasks[Pierwszy].N;
            if (Iteracja != OstIteracja)
            {
                Zadanie = Tasks[Pierwszy];
                Pierwszy = Zadanie.Forget();    // Funkcja zwraca numer nastepnego zadania
                Iteracja++;
                LB = Bound3(Pierwszy);
                if (LB <= NajlepszyCzas)
                {
                    Rekurencje++;
                    BnB3(Pierwszy);
                }
                Iteracja--;
                Zadanie.Remind();
            }
            else
            {
                Pierwszy = Tasks[Pierwszy].Next.N;
                Permutacja[Iteracja + 1] = Tasks[Pierwszy].N;
                PoliczPermutacje(); // Funkcja załatwia sprawe z uaktualnieniem UB i zapisania permutacji

            }
        }
    }

    void BnB4(int Pierwszy)
    {
        Task Zadanie;
        int LB;
        for (int i = TasksCount - Iteracja; i > 0; i--)
        {
            Permutacja[Iteracja] = Tasks[Pierwszy].N;
            if (Iteracja != OstIteracja)
            {
                Zadanie = Tasks[Pierwszy];
                Pierwszy = Zadanie.Forget();    // Funkcja zwraca numer nastepnego zadania
                Iteracja++;
                LB = Bound4(Pierwszy);
                if (LB <= NajlepszyCzas)
                {
                    Rekurencje++;
                    BnB4(Pierwszy);
                }
                Iteracja--;
                Zadanie.Remind();
            }
            else
            {
                Pierwszy = Tasks[Pierwszy].Next.N;
                Permutacja[Iteracja + 1] = Tasks[Pierwszy].N;
                PoliczPermutacje(); // Funkcja załatwia sprawe z uaktualnieniem UB i zapisania permutacji

            }
        }
    }

    int LBMax;
    int CP;
    Task TmpTesk;
    int Bound1(int Pierwszy)
    {
        LBMax = 0;
        Czas = 0;
        // Utworzenie pierwszego wiersza zakonczen dla pierwszej maszyny
        CP = 0;
        TmpTesk = Tasks[Pierwszy];      // pierwsze Zadanie uszeregowane
        for (int b = 0; b < TasksCount; b++)  // Po wszystkich zadaniach
        {
            if (b < Iteracja)   // Czesc dla uszeregowanych
            {
                CP = Zakonczenia[b, 0] = CP + Tasks[Permutacja[b]].P[0];
            } else              // Czesc dla nieuszeregowanych
            {
                CP += TmpTesk.P[0];
                TmpTesk = TmpTesk.Next;
            }
        }
        LBMax = Mathf.Max(LBMax, CP);

        // Dla kolejnych maszyn
        for (int a = 1; a < DevicesCount; a++)  // Dla kazdej kolejnej maszyny
        {
            CP = 0;
            TmpTesk = Tasks[Pierwszy];      // pierwsze Zadanie uszeregowane
            for (int b = 0; b < TasksCount; b++)  // Po wszystkich zadaniach
            {
                if (b < Iteracja)   // Czesc dla uszeregowanych
                {
                    CP = Zakonczenia[b, a] = Tasks[Permutacja[b]].P[a] + Mathf.Max(Zakonczenia[b, a - 1], CP);
                }
                else              // Czesc dla nieuszeregowanych
                {
                    CP += TmpTesk.P[a];
                    TmpTesk = TmpTesk.Next;
                }
            }
            LBMax = Mathf.Max(LBMax, CP);
        }
        return LBMax;
    }

    float minp;
    int[] TrzeciCzlon;
    int Bound3(int Pierwszy)
    {
        LBMax = 0;
        Czas = 0;
        // Utworzenie pierwszego wiersza zakonczen dla pierwszej maszyny
        CP = 0;
        TmpTesk = Tasks[Pierwszy];      // pierwsze Zadanie uszeregowane
        for (int b = 0; b < TasksCount; b++)  // Po wszystkich zadaniach
        {
            if (b < Iteracja)   // Czesc dla uszeregowanych
            {
                CP = Zakonczenia[b, 0] = CP + Tasks[Permutacja[b]].P[0];
            }
            else              // Czesc dla nieuszeregowanych
            {
                CP += TmpTesk.P[0];
                TmpTesk = TmpTesk.Next;
            }
        }
        // Wyliczenie trzeciego członu
        for (int c = 1; c < DevicesCount; c++)  // Dla kazdej kolejnej maszyny
        {
            minp = Mathf.Infinity;
            TmpTesk = Tasks[Pierwszy];      // pierwsze Zadanie uszeregowane
            for (int d = Iteracja; d < TasksCount; d++)  // Po nieuszeregowanych
            {
                minp = Mathf.Min(minp, TmpTesk.P[c]);
                TmpTesk = TmpTesk.Next;
            }
            CP += TrzeciCzlon[c] = (int)minp;   // dla 0 maszyny od razu wykorzystamy te dane
        }

        LBMax = Mathf.Max(LBMax, CP);
        // Dla kolejnych maszyn
        for (int a = 1; a < DevicesCount; a++)  // Dla kazdej kolejnej maszyny
        {
            CP = 0;
            TmpTesk = Tasks[Pierwszy];      // pierwsze Zadanie uszeregowane
            for (int b = 0; b < TasksCount; b++)  // Po wszystkich zadaniach
            {
                if (b < Iteracja)   // Czesc dla uszeregowanych
                {
                    CP = Zakonczenia[b, a] = Tasks[Permutacja[b]].P[a] + Mathf.Max(Zakonczenia[b, a - 1], CP);
                }
                else              // Czesc dla nieuszeregowanych
                {
                    CP += TmpTesk.P[a];
                    TmpTesk = TmpTesk.Next;
                }
            }
            // Tzreci człon
            for (int e = a + 1; e < DevicesCount; e++)
            {
                CP += TrzeciCzlon[e];
            }
            LBMax = Mathf.Max(LBMax, CP);
        }
        return LBMax;
    }
    int sump;
    int Bound4(int Pierwszy)
    {
        LBMax = 0;
        Czas = 0;
        // Utworzenie pierwszego wiersza zakonczen dla pierwszej maszyny
        minp = Mathf.Infinity;
        CP = 0;
        TmpTesk = Tasks[Pierwszy];      // pierwsze Zadanie uszeregowane
        for (int b = 0; b < TasksCount; b++)  // Po wszystkich zadaniach
        {
            if (b < Iteracja)   // Czesc dla uszeregowanych
            {
                CP = Zakonczenia[b, 0] = CP + Tasks[Permutacja[b]].P[0];
            }
            else              // Czesc dla nieuszeregowanych
            {
                CP += TmpTesk.P[0];
                // Wyliczenie trzeciego członu
                sump = 0;
                for (int c = 1; c < DevicesCount; c++)  // Dla kazdej kolejnej maszyny
                {
                    sump += TmpTesk.P[c];
                }
                minp = Mathf.Min(minp, sump);

                TmpTesk = TmpTesk.Next;
            }
        }
        CP += (int)minp;
        LBMax = Mathf.Max(LBMax, CP);
        // Dla kolejnych maszyn
        for (int a = 1; a < DevicesCount; a++)  // Dla kazdej kolejnej maszyny
        {
            CP = 0;
            minp = Mathf.Infinity;
            TmpTesk = Tasks[Pierwszy];      // pierwsze Zadanie uszeregowane
            for (int b = 0; b < TasksCount; b++)  // Po wszystkich zadaniach
            {
                if (b < Iteracja)   // Czesc dla uszeregowanych
                {
                    CP = Zakonczenia[b, a] = Tasks[Permutacja[b]].P[a] + Mathf.Max(Zakonczenia[b, a - 1], CP);
                }
                else              // Czesc dla nieuszeregowanych
                {
                    CP += TmpTesk.P[a];
                    // Tzreci człon
                    sump = 0;
                    for (int c = a + 1; c < DevicesCount; c++)  // Dla kazdej kolejnej maszyny
                    {
                        sump += TmpTesk.P[c];
                    }
                    minp = Mathf.Min(minp, sump);

                    TmpTesk = TmpTesk.Next;
                }
            }
            CP += (int)minp;
            LBMax = Mathf.Max(LBMax, CP);
        }
        return LBMax;
    }

    int Johnson()
    {
        List<Task> tasks = new List<Task>(Tasks);
        tasks.Sort(
          delegate (Task t1, Task t2)
          {
              int a;
              float p1 = Mathf.Infinity;
              for (a = 0; a < t1.P.Length; a++)
              {
                  if (p1 > t1.P[a]) p1 = t1.P[a];
              }
              float p2 = Mathf.Infinity;
              for (a = 0; a < t2.P.Length; a++)
              {
                  if (p2 > t2.P[a]) p2 = t2.P[a];
              }
              int res = p1.CompareTo(p2);
              
              return res;
          }
      );
        List<Task> begin = new List<Task>();
        List<Task> end = new List<Task>();
        for (int i = 0; i < TasksCount; i++)
        {
            if (tasks[i].P[0] < tasks[i].P[DevicesCount - 1]) begin.Add(tasks[i]);
            else end.Add(tasks[i]);
        }
        for (int a = end.Count; a > 0; a--)
        {
            begin.Add(end[a - 1]);
        }
        for (int i = 0; i < TasksCount; i++)
        {
            Permutacja[i] = begin[i].N;
        }
        return PoliczPermutacje();
    }

    int Czas;
    public int PoliczPermutacje()
    {
        Czas = 0;
        for (int i = 0; i < TasksCount; i++)
        {
            Czas = Zakonczenia[i, 0] = Czas + Tasks[Permutacja[i]].P[0];
        }
        for (int a = 1; a < DevicesCount; a++)
        {
            Czas = 0;
            for (int i = 0; i < TasksCount; i++)
            {
                Czas = Zakonczenia[i, a] = Tasks[Permutacja[i]].P[a] + Mathf.Max(Zakonczenia[i, a - 1], Czas);
            }
        }
        if (Czas < NajlepszyCzas)
        {
            NajlepszyCzas = Czas;
            Array.Copy(Permutacja, NajlepszaPerm, TasksCount);
        }
        return Czas;
    }

    public int PoliczPermutacjeDo(int Count)
    {
        Czas = 0;
        for (int i = 0; i <= Count; i++)
        {
            Czas = Zakonczenia[i, 0] = Czas + Tasks[Permutacja[i]].P[0];
        }
        for (int a = 1; a < DevicesCount; a++)
        {
            Czas = 0;
            for (int i = 0; i <= Count; i++)
            {
                Czas = Zakonczenia[i, a] = Tasks[Permutacja[i]].P[a] + Mathf.Max(Zakonczenia[i, a - 1], Czas);
            }
        }
        return Czas;
    }
    public int NumerZadaniaZKryterium(int Count, int zI)
    {
        Czas = 0;
        for (int i = 0; i <= Count; i++)
        {
            Czas = Zakonczenia[i, 0] = Czas + Tasks[Permutacja[i]].P[0];
        }
        for (int a = 1; a < DevicesCount; a++)
        {
            Czas = 0;
            for (int i = 0; i <= Count; i++)
            {
                Czas = Zakonczenia[i, a] = Tasks[Permutacja[i]].P[a] + Mathf.Max(Zakonczenia[i, a - 1], Czas);
            }
        }
        int taskNum = Count;
        int devNum = DevicesCount - 1;
        int CLeft, CUpper;
        int NumerZadania;
        float Kryterium = Mathf.Infinity;
        int tmp;
        Task t1;
        // Policzenie kryterium
        t1 = Tasks[Permutacja[taskNum]];
        Kryterium = PoliczKryterium();
        NumerZadania = t1.N;
        while (true)
        {
            // Przejscie do nastepnego zadania w sciezce
            if (taskNum - 1 >= 0) CLeft = Zakonczenia[taskNum - 1, devNum];
            else CLeft = 0;
            if (devNum - 1 >= 0) CUpper = Zakonczenia[taskNum, devNum - 1];
            else CUpper = 0;
            if (CLeft > CUpper) taskNum--;
            else devNum--;
            // Policzenie kryterium
            t1 = Tasks[Permutacja[taskNum]];
            if (zI != t1.N)     // Sprawdzenie czy nie ma 
            {
                tmp = PoliczKryterium();
                if (tmp > Kryterium)
                {
                    Kryterium = tmp;
                    NumerZadania = t1.N;
                }
            }
            // Sprawdzenie czy nie doszło sie do konca
            if ((taskNum - 1 < 0) && (devNum - 1 < 0)) break;
        }
        return NumerZadania;

        int PoliczKryterium()
        {
            float p1 = Mathf.Infinity;
            for (int a = 0; a < t1.P.Length; a++)
            {
                if (p1 > t1.P[a]) p1 = t1.P[a];
            }
            return (int)p1;
        }
    }

    int OstIteracja, Iteracja;
    void BruteForce(int Pierwszy)
    {
        int Zapomniany;
        // Wykonujemy tyle razy ile zostalo juz elementow
        for (int i = TasksCount - Iteracja; i > 0; i--)
        {
            // Wpisujemy do permutacji numerek naszego zadania
            Permutacja[Iteracja] = Tasks[Pierwszy].N;
            // Sprawdzamy czy nastepne wywołanie nie było by ostatnim 
            if (Iteracja != OstIteracja)
            {
                Zapomniany = Pierwszy;
                // Nasze zadanie zapomianamy z kolejki dwukierunkowej
                Pierwszy = Tasks[Pierwszy].Forget();    // Funkcja zwraca numer nastepnego zadania
                // Wywołanie kolejnej rekurencji, dla niej pierwszy element jest następnym od naszego
                Iteracja++;
                BruteForce(Pierwszy);
                // Cofanie zmian
                Tasks[Zapomniany].Remind();
                // Cofamy sie z iteracja
                Iteracja--;
            }
            else
            {
                // Nastepne zadanie
                Pierwszy = Tasks[Pierwszy].Next.N;
                // Uzupełniamy ostatni element permutacji
                Permutacja[Iteracja + 1] = Tasks[Pierwszy].N;
                // Policz permutacje
                PoliczPermutacje();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
