using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class Task
{
    public int N;
    public int R;
    public int P;
    public int Q;
    public Task(int n, int r, int p, int q)
    {
        N = n;
        R = r;
        P = p;
        Q = q;
    }

    public Task(Task task)
    {
        N = task.N;
        R = task.R;
        P = task.P;
        Q = task.Q;
    }
}
public class Lab_2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int Count, K;
        string[] path = { 
            "Assets/Resources/data10.txt" ,
            "Assets/Resources/data20.txt" ,
            "Assets/Resources/data50.txt" ,
            "Assets/Resources/data100.txt",
            "Assets/Resources/data200.txt",
            "Assets/Resources/data500.txt"
        };

        for (int a = 0; a < 6; a++) {
            List<Task> Tasks = new List<Task>();
            List<Task> tasks = new List<Task>();
            StreamReader Reader = new StreamReader(path[a]);
            var Content = Reader.ReadToEnd();
            Reader.Close();
            var Lines = Content.Split("\n"[0]);

            string[] stringSeparators = new string[] { @"\tab", " " };
            var FirstLine = Lines[0].Split(stringSeparators, 0);
            //Debug.Log("_"+Lines[0]+"_");
            Count = int.Parse(FirstLine[0]);
            K = int.Parse(FirstLine[1]);
            //Debug.Log(Count);
            //Debug.Log(K);


            for (int i = 1; i <= Count; i++)
            {
                //Debug.Log(Lines[i]);
                var Line = Lines[i].Split(" "[0]);
                Task task = new Task(i, int.Parse(Line[0]), int.Parse(Line[1]), int.Parse(Line[2]));     // Zadania są od 1 
                Tasks.Add(task);

            }

            Stopwatch SortRQTime = new Stopwatch();
            Stopwatch SchrageTime = new Stopwatch();
            Stopwatch CarlierTime = new Stopwatch();
            for (int i = 0; i < 1000; i++)
            {
                tasks = CopyTasks(Tasks);

                SortRQTime.Start();
                SortRQ(tasks);
                SortRQTime.Stop();

                tasks = CopyTasks(Tasks);

                SchrageTime.Start();
                Schrage(tasks, false);// nie edytuje listy
                SchrageTime.Stop();

                tasks = CopyTasks(Tasks);

                CarlierTime.Start();
                Carlier(tasks);
                CarlierTime.Stop();

            }
            float SortRQMS = SortRQTime.ElapsedMilliseconds / 1000f;
            float SchrageMS = SchrageTime.ElapsedMilliseconds / 1000f;
            float CarlierMS = CarlierTime.ElapsedMilliseconds / 1000f;

            tasks = CopyTasks(Tasks);
            int SortRQCmax = GetCmax(SortRQ(tasks));
            tasks = CopyTasks(Tasks);
            int SchrageCmax = GetCmax(Schrage(tasks, false));
            tasks = CopyTasks(Tasks);
            int CarlierCmax = GetCmax(Carlier(tasks));

            UnityEngine.Debug.Log(
                tasks.Count 
                + " & " + SortRQCmax
                + " & " + SchrageCmax
                + " & " + CarlierCmax
                + " & " + PRD(SortRQCmax, CarlierCmax) 
                + " & " + PRD(SchrageCmax, CarlierCmax) 
                + " & " + SortRQMS 
                + " & " + SchrageMS 
                + " & " + CarlierMS 
                + " \\");
            
        }
    }

    float PRD(int Cmax, int Cref)
    {
        return 100f * (Cmax - Cref) / Cmax;
    }

    List<Task> CopyTasks(List<Task> CopyFrom)
    {
        List<Task>  CopyTo = new List<Task>();
        foreach (Task task in CopyFrom)
        {
            CopyTo.Add(new Task(task));
        }

        return CopyTo;
    }

    string PrintC(int[] array)
    {
        string s = "";
        foreach (int val in array)
        {
            s += val + " ";
        }
        return s;
    }

    int GetCmax(int[] array)
    {
        return array[array.Length -1];
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ZADANIE 1
    int[] CMax(List<Task> Tasks)
    {
        int Count = Tasks.Count;
        int[] S = new int[Count];
        int[] C = new int[Count + 1];
        int Cmax = 0;

        S[0] = Tasks[0].R;              // Czas przygotowania   1 zadania
        C[0] = S[0] + Tasks[0].P;       // Czas zakonczenia     1 zadania
        Cmax = C[0] + Tasks[0].Q;       // Czas maksymalny zakonczenia

        for (int i = 1; i < Count; i++)
        {
            S[i] = Mathf.Max(Tasks[i].R, C[i - 1]);     // Czas przygotowania nastepnego zadania to max z czasu jego przygotowania lub wykonania wczesniejszego zadania
            C[i] = S[i] + Tasks[i].P;
            Cmax = Mathf.Max(Cmax, C[i] + Tasks[i].Q);
        }
        //Debug.Log(Cmax);
        C[Count] = Cmax;
        return C;
    }

    int[] SortRQ(List<Task> Tasks)
    {
        Tasks.Sort(
           delegate (Task t1, Task t2)
           {
               int res = t1.R.CompareTo(t2.R);
               if (res == 0)
               {
                   res = t2.Q.CompareTo(t1.Q);
               }
               return res;
           }
       );
        return CMax(Tasks);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // ZADANIE 2

    // Funkcja zwraca tablice C + Cmax, zmienna Modify sprawia ze
    // true  => modyfikuje listę, lecz tylko referencje, same zadania maja te same adresy, nie sa przenoszone ani nic
    // false => nie modyfikuje listy, same zadania maja te same adresy, nie sa przenoszone ani nic
    int[] Schrage(List<Task> Tasks, bool Modify)
    {
        //int k = 0;
        int j = 0;
        List<Task> G = new List<Task>();
        List<Task> N = new List<Task>(Tasks);
        List<Task> tasks = new List<Task>();
        if (Modify) Tasks.Clear();
        //List<Task> N;
        int t = MinR(N);
        //Debug.Log(t);
        while ((G.Count != 0) || (N.Count != 0))    // Dopuki mamy zadania nieuszeregowane w G lub N
        {
            while ((N.Count != 0) && (MinR(N) <= t))    // Dopuki mamy zadania w N i posiadają one jakiś czas 'R' mniejszy od 't'
            {
                j = MinRNum(N);
                G.Add(N[j]);
                N.RemoveAt(j);
            }
            if (G.Count != 0)
            {
                j = MaxQNum(G);
                if (Modify) Tasks.Add(G[j]);
                else tasks.Add(G[j]);
                t += G[j].P;
                //k += 1;
                G.RemoveAt(j);
            }
            else
            {
                t = MinR(N);
            }
        }
        if (Modify) return CMax(Tasks);
        else return CMax(tasks);
    }

    // Funkcja zwraca Cmax, nie modyfikuje listy, same zadania maja te same adresy, nie sa przenoszone ani nic
    int SchragePmtn(List<Task> Tasks)
    {
        //int k = 0;
        int j = 0;
        int Cmax = 0;
        List<Task> G = new List<Task>();
        List<Task> N = new List<Task>(Tasks);
        Task task = new Task(0, 0, 0, 0);
        //List<Task> N;
        int t = MinR(N);
        //Debug.Log(t);
        while ((G.Count != 0) || (N.Count != 0))    // Dopuki mamy zadania nieuszeregowane w G lub N
        {
            while ((N.Count != 0) && (MinR(N) <= t))    // Dopuki mamy zadania w N i posiadają one jakiś czas 'R' mniejszy od 't'
            {
                j = MinRNum(N);
                G.Add(N[j]);

                if (N[j].Q > task.Q)
                {
                    task.P = t - N[j].R;
                    t = N[j].R;
                    if (task.P > 0)
                    {
                        N.Add(task);
                    }
                }

                N.RemoveAt(j);
            }
            if (G.Count != 0)
            {
                j = MaxQNum(G);
                task = new Task(G[j]);
                t += G[j].P;
                //k += 1;
                Cmax = Mathf.Max(Cmax, t + task.Q);
                G.RemoveAt(j);
            }
            else
            {
                t = MinR(N);
            }
        }
        return Cmax;
    }


    int MinR(List<Task> N)
    {
        float r = Mathf.Infinity;
        foreach (Task task in N)
        {
            if (task.R < r) r = task.R;
        }
        return (int)r;
    }

    int MaxQNum(List<Task> Tasks)       // Zwraca numer zadania w liscie z najwiekszym Q
    {
        int q = 0;
        int num = 0;
        for (int i = 0; i < Tasks.Count; i++)
        {
            if (Tasks[i].Q >= q)
            {
                q = Tasks[i].Q;
                num = i;
            }
        }
        return num;
    }

    int MinRNum(List<Task> Tasks)       // Zwraca numer zadania w liscie z najwiekszym Q
    {
        float r = Mathf.Infinity;
        int num = 0;
        for (int i = 0; i < Tasks.Count; i++)
        {
            if (Tasks[i].R <= r)
            {
                r = Tasks[i].R;
                num = i;
            }
        }
        return num;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ZADANIE 3
    int[] Carlier(List<Task> tasks)
    {
        float UB = Mathf.Infinity;
        List<Task> Optimal = new List<Task>();
        List<Task> K;
        int[] C;
        int a, b, c;
        float r;
        int Cmax, LB;
        int length = tasks.Count;

        carlier(tasks);

        void carlier(List<Task> Tasks)
        {
            float p, q;
            Task TempTask;
            int OldVal;

            C = Schrage(Tasks, true);
            Cmax = C[length];
            if (Cmax < UB)
            {
                UB = Cmax;
                Optimal = new List<Task>(Tasks);
            }
            b = FindB(Tasks);
            a = FindA(Tasks);
            c = FindC(Tasks);
            if (c == -1)
            {
                return;
            }
            K = new List<Task>();
            for (int i = c + 1; i <= b; i++)
            {
                K.Add(Tasks[i]);      
            }
            r = q = Mathf.Infinity;
            p = 0;
            foreach (Task task in K)
            {
                if (task.R < r) r = task.R;
                if (task.Q < q) q = task.Q;
                p += task.P;
            }
            TempTask = Tasks[c];  
            OldVal = TempTask.R;
            TempTask.R = Mathf.Max(TempTask.R, (int)(r + p));
            LB = SchragePmtn(Tasks);
            if (LB < UB)
            {
                carlier(Tasks);
            }
            TempTask.R = OldVal;

            OldVal = TempTask.Q;
            TempTask.Q = Mathf.Max(TempTask.Q, (int)(q + p));
            LB = SchragePmtn(Tasks);
            if (LB < UB)
            {
                carlier(Tasks);
            }
            TempTask.Q = OldVal;
        }

        int FindB(List<Task> Tasks)
        {
            int tmp;
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                tmp = Tasks[i].Q + C[i];
                if (tmp == Cmax)
                {
                    num = i;
                }
            }
            return num;
        }

        int FindA(List<Task> Tasks)
        {
            int tmp;
            int num = 0;
            int i = length - 1;
            for (; i >= 0; i--)
            {
                tmp = 0;
                for (int x = i; x <= b; x++)       // Suma P dla zadan od 'i' do 'b'
                {
                    tmp += Tasks[x].P;
                }
                tmp += Tasks[i].R + Tasks[b].Q;
                if (tmp == Cmax)
                {
                    num = i;
                }
            }
            return num;
        }

        int FindC(List<Task> Tasks)
        {
            int num = -1;
            for (int i = a; i < b; i++)
            {
                if (Tasks[i].Q < Tasks[b].Q) num = i;
            }
            return num;
        }

        return CMax(Optimal);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
