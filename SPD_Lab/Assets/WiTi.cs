using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;

public class WiTi : MonoBehaviour
{
    public class Task
    {
        public int N;   // Numer zadania
        public int P;   // Czas wykonania
        public int W;   // Waga
        public int D;   // Termin zakończenia
        public Task Next;
        public Task Prev;

        // Wczesniejszy elemenmt wskazuje na nastepny zamiast ten, 
        // Nastepny za to do tyłu wskazuje na nasz poprzedni zamiast ten,
        // Dodatkowo zwrcamy numer nastepnego od tego zadania
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
        public Task(int n, int p, int w, int d)
        {
            N = n;
            P = p;
            W = w;
            D = d;
        }
    }

    List<Task> Tasks;
    int[] Permutacja, NajlepszaPerm;
    int Count;
    float NajlepszaKara;
    // Start is called before the first frame update
    void Start()
    {
        int K;
        string[] path = {
            "Assets/WiTi/data10.txt" ,
            "Assets/WiTi/data11.txt" ,
            "Assets/WiTi/data12.txt" ,  // [2] ze 3 minuty
            "Assets/WiTi/data13.txt" ,
            "Assets/WiTi/data14.txt" ,
            "Assets/WiTi/data15.txt" ,
            "Assets/WiTi/data16.txt" ,
            "Assets/WiTi/data17.txt" ,
            "Assets/WiTi/data18.txt" ,
            "Assets/WiTi/data19.txt" ,
            "Assets/WiTi/data20.txt" 
        };

        for (int a = 0; a < path.Length; a++)
        //int a = 0;
        {
            Tasks = new List<Task>();
            StreamReader Reader = new StreamReader(path[a]);
            var Content = Reader.ReadToEnd();
            Reader.Close();
            //UnityEngine.Debug.Log(Content);
            var Lines = Content.Split("\n"[0]);
            string[] stringSeparators = new string[] { @"\tab", " " };
            var FirstLine = Lines[0].Split(stringSeparators, 0);
            //UnityEngine.Debug.Log("_"+Lines[0]+"_");
            Count = int.Parse(FirstLine[0]);
            Permutacja = new int[Count];
            NajlepszaPerm = new int[Count];
            K = int.Parse(FirstLine[1]);
            //UnityEngine.Debug.Log(Count);
            //UnityEngine.Debug.Log(K);

            for (int i = 0; i < Count; i++)
            {
                //UnityEngine.Debug.Log(Lines[i]);
                var Line = Lines[i+1].Split(" "[0]);
                Task task = new Task(i, int.Parse(Line[0]), int.Parse(Line[1]), int.Parse(Line[2]));     // Zadania są od 1 
                Tasks.Add(task);

            }

            Stopwatch stopwatch = new Stopwatch();

            /*
            for (int b = 0; b < Count; b++)
            {
                Permutacja[b] = b;
            }
            NajlepszaKara = Mathf.Infinity;

            PoliczPermutacje();
            UnityEngine.Debug.Log("Kara Natur: " + NajlepszaKara.ToString());

            SortD();
            PoliczPermutacje();
            UnityEngine.Debug.Log("Kara SortD: " + NajlepszaKara.ToString());
            */

            NajlepszaKara = Mathf.Infinity;
            for (int i = 0; i < Count - 1; i++)
            {
                Tasks[i].Next = Tasks[i + 1];
            }
            Tasks[Count - 1].Next = Tasks[0];
            for (int i = 1; i < Count; i++)
            {
                Tasks[i].Prev = Tasks[i - 1];
            }
            Tasks[0].Prev = Tasks[Count - 1];
            /*
            OstIteracja = Count - 2;
            Iteracja = 0;
            stopwatch.Start();
            BruteForce(0);
            stopwatch.Stop();
            UnityEngine.Debug.Log("Kara SortD: " + NajlepszaKara.ToString() + " Czas: " + stopwatch.ElapsedMilliseconds);
            */
            
            OstIteracja = Count - 2;
            Iteracja = 0;
            WielkaBazaZbiorow = new bool[(int)Mathf.Pow(2, Count + 1) + 1];
            // Inicjalizacja tablicy
            for (int x = 0, y = WielkaBazaZbiorow.Length; x < y; x++){
                WielkaBazaZbiorow[x] = false;
            }
            KaraPodzbioru = new int[(int)Mathf.Pow(2, Count + 1) + 1];
            stopwatch.Start();
            ZborJuzByl(0);
            NajlepszaKara = Dynamiczny(0, CzasPodzbioru);
            stopwatch.Stop();
            UnityEngine.Debug.Log("Kara SortD: " + NajlepszaKara.ToString() + " Czas: " + stopwatch.ElapsedMilliseconds);
            
        }
    }

    int c;
    double Zbior;
    bool[] WielkaBazaZbiorow;
    int CzasPodzbioru;
    ///int KaraZbioru;
    // Funkcyja liczy czas dla zbioru i sprawdza czy on sobie jest juz zapisany czy nie;
    bool ZborJuzByl(int Pierwszy)
    {
        CzasPodzbioru = 0;
        Zbior = 0;
        zadanie = Tasks[Pierwszy];
        for (c =  Count - Iteracja; c > 0; c--)
        {
            Zbior += Mathf.Pow(2,zadanie.N);
            CzasPodzbioru += zadanie.P;
            zadanie = zadanie.Next;
        }
        // Sprawdzenie czy dany zbor juz istnieje
        if (WielkaBazaZbiorow[(int)Zbior] == true) return true;

        WielkaBazaZbiorow[(int)Zbior] = true;
        return false;
    }


    int[] KaraPodzbioru;
    int KaraZbioru, karaPodzbioru;
    int Dynamiczny(int Pierwszy, int CzasZbioru)
    {
        Task Zadanie;
        float KaraMin = Mathf.Infinity;
        int MiejsceWBazie;
        for (int i = Count - Iteracja; i > 0; i--)
        {
            if (Iteracja != OstIteracja)    //przedostatnie wywołanie bedzie ostatnim
            {
                Zadanie = Tasks[Pierwszy];
                Pierwszy = Zadanie.Forget();    // Funkcja zwraca numer nastepnego zadania
                Iteracja++;
                if (!ZborJuzByl(Pierwszy))
                {
                    // Zbioru nie było
                    MiejsceWBazie = (int)Zbior;
                    KaraPodzbioru[MiejsceWBazie] = Dynamiczny(Pierwszy, CzasPodzbioru);
                    KaraZbioru = Mathf.Max(CzasZbioru - Zadanie.D, 0) * Zadanie.W;
                    KaraMin = (int)Mathf.Min((KaraZbioru + KaraPodzbioru[MiejsceWBazie]), KaraMin);
                }
                else
                {
                    KaraZbioru = Mathf.Max(CzasZbioru - Zadanie.D, 0) * Zadanie.W;
                    KaraMin = (int)Mathf.Min((KaraZbioru + KaraPodzbioru[(int)Zbior]), KaraMin); // c wskazuje na miejsce gdzie funkcja ZbiorJuzByl znalazla zbior
                }
                Iteracja--;
                Zadanie.Remind();
            }
            else
            {
                Zadanie = Tasks[Pierwszy];
                KaraZbioru = Mathf.Max(CzasZbioru - Zadanie.D, 0) * Zadanie.W;
                Zadanie = Zadanie.Next;
                Pierwszy = Zadanie.N;
                karaPodzbioru = Mathf.Max(Zadanie.P - Zadanie.D, 0) * Zadanie.W;
                KaraMin = (int)Mathf.Min((KaraZbioru + karaPodzbioru), KaraMin);
            }
        }
        return (int)KaraMin;
    }

    int a, czas;
    float Kara;
    Task zadanie;
    void PoliczPermutacje()
    {
        a = 0;
        czas = 0;
        Kara = 0;
        for (; a < Count; a++)
        {
            // zapamiętujemy zadanie
            zadanie = Tasks[Permutacja[a]];
            // dodajemy czas wykonaia tego zadania
            czas += zadanie.P;
            // Sprawdzamy czy jestesmy spoznieni
            if (czas > (zadanie.D))
            {
                // Spoznilimy sie
                Kara += zadanie.W * (czas - zadanie.D);
            }
        }
        if (Kara < NajlepszaKara)
        {
            NajlepszaKara = Kara;
            // Kopiowac mozna na 2 sposoby, ktory szybszy ?? 
            Array.Copy(Permutacja, NajlepszaPerm, Count);
            //NajlepszaPerm = (int[])Permutacja.Clone();
        }
    }

    void SortD()
    {
        Tasks.Sort(
          delegate (Task t1, Task t2)
          {
              int res = t1.D.CompareTo(t2.D);
              // Dodatkow sortujemy po W
              if (res == 0)
              {
                  res = t2.W.CompareTo(t1.W);
              }
              return res;
          }
      );
    }

    int OstIteracja, Iteracja;
    void BruteForce(int Pierwszy)
    {
        int Zapomniany;
        // Wykonujemy tyle razy ile zostalo juz elementow
        for (int i = Count - Iteracja; i > 0 ; i --)
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
