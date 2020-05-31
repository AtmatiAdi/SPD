using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using UnityEngine.UI;
using UnityEngine.Networking;

public class plan : MonoBehaviour
{
    public Text text;
    string[] Nauczyciele = new string[]{ "Nauczyciel_1", "Nauczyciel_2", "Nauczyciel_3" };
    string[] Przedmioty = new string[]{ "Matematyka", "Polski", "Biologia" };
    int[,] NauczycielePrzedmioty = new int[,] { { 1, 1, 0 }, { 1, 0, 1 }, { 1, 0, 0 } };
    string[] Sale = new string[] { "Sala_1", "Sala_2", "Sala_3" };
    int[,] SalePrzedmioty = new int[,] { { 1, 1, 1 }, { 0, 1, 1 }, { 1, 1, 0 } };
    string[] Grupy = new string[] { "A1", "B1" };
    string[] Potoki = new string[] { "Pierwsza" };
    int[,] PotokiPrzedmioty = new int[,] { { 2, 2, 2 } };
    int[,] PotokiGrupy = new int[,] { { 1, 1 } };
    int[,] GrupyPrzedmioty = new int[,] { { 2, 2, 2 }, { 2, 2, 2 } };
    int[] Godziny = new int[] { 1, 2, 3 };
    List<W> Zajecia = new List<W>();
    class W {
        public W(int grupa, int nauczyciel, int sala, int przedmiot, int godzina)
        {
            Grupa = grupa;
            Nauczyciel = nauczyciel;
            Sala = sala;
            Przedmiot = przedmiot;
            Godzina = godzina;
            Kolor = 0;
        }

        public int Grupa;
        public int Nauczyciel;
        public int Sala;
        public int Przedmiot;
        public int Godzina;
        public uint Kolor;
    }
    List<W> Graf = new List<W>();
    bool[,] Polaczenia;

    void AddToLog(string msg)
    {
        text.text = text.text + msg;
    }

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            string conString = "User Id=M;Password=oracle;" +
                    "Data Source=25.109.227.82:1522/xe;";

            OracleConnection con = new OracleConnection();
            con.ConnectionString = conString;
            con.Open();
            AddToLog("Połączono...");

            OracleCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from szkoly";
            //cmd.CommandText = "insert into szkoly values (2,'test3')";
            //cmd.CommandType = CommandType.Text;
            //cmd.ExecuteNonQuery();
            //cmd.CommandText = "commit";
            AddToLog("Zapytano");
            OracleDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    //Console.WriteLine("Odebrano: " + reader.GetString(0));
                    AddToLog("Odebrano: ");
                    AddToLog(reader.GetInt32(0) + ">" + reader.GetString(1));
                }
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Debug.Log(ex.InnerException);
            Debug.Log(ex.Data);
        }
        // Generowanie wierzchołków
        //for (int godzina = 0; godzina < Godziny.Length; godzina++) 
        {
            for (int grupa = 0; grupa < Grupy.Length; grupa++)
            {
                for (int nauczyciel = 0; nauczyciel < Nauczyciele.Length; nauczyciel++)
                {
                    for (int sala = 0; sala < Sale.Length; sala++)
                    {
                        for (int przedmiot = 0; przedmiot < Przedmioty.Length; przedmiot++)
                        {
                            for (int potok = 0; potok < Potoki.Length; potok++) {
                                // Jezeli Grupa ma dany Potok
                                if (PotokiGrupy[potok, grupa] > 0)
                                {
                                    // Jezeli Potok ma dany Przedmiot
                                    if (PotokiPrzedmioty[potok, przedmiot] > 0)
                                    {
                                        // Jezeli Nauczyciel ma Przedmiot
                                        if (NauczycielePrzedmioty[nauczyciel, przedmiot] > 0)
                                        {
                                            // Jezeli Sala ma Przedmiot
                                            if (SalePrzedmioty[sala, przedmiot] > 0)
                                            {
                                                W nowy = new W(grupa, nauczyciel, sala, przedmiot, 0);
                                                Graf.Add(nowy);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        int Count;
        int Godzina = 1;
        while (Graf.Count > 0)
        {
            // Polaczenia
            Count = Graf.Count;
            Poloaczenia();
            // Kolorowanie
            ColorCount = 0;
            Kolorowanie();
            if (ColorCount == 0) break;
            // Zamiana grafu na plan
            GrafNaPlan();
            Debug.Log("Ilosc Kolorw: " + ColorCount);
        }
        foreach (W w in Zajecia)
        {
            PrintW(w);
        }

// FUNKCJE FUNCKJE FUNKCJE FUNKCJE FUNKCJE

        void Poloaczenia()
        {
            Polaczenia = new bool[Count, Count];
            for (int w1 = 0; w1 < Count; w1++)
            {
                for (int w2 = 0; w2 < Count; w2++)
                {
                    // Jezeli w tej samej godzinie
                    //if (Graf[w1].Godzina == Graf[w2].Godzina) { 
                    // W tej samej godzinie 
                    if (
                    Graf[w1].Grupa == Graf[w2].Grupa ||
                    Graf[w1].Nauczyciel == Graf[w2].Nauczyciel ||
                    Graf[w1].Sala == Graf[w2].Sala)
                    {
                        Polaczenia[w1, w2] = Polaczenia[w2, w1] = true;
                    }
                    //}
                }
            }
        }

        void Kolorowanie()
        {
            uint KolorySasiadow = 0;
            for (int w = 0; w < Graf.Count; w++)
            {
                // Pobranie kolorow wszystkich sasiadow
                for (int sasiad = 0; sasiad < Graf.Count; sasiad++)
                {
                    // Polaczony z w
                    if (Polaczenia[w, sasiad])
                    {
                        KolorySasiadow = SumColor(KolorySasiadow, Graf[sasiad].Kolor);
                    }
                }
                Graf[w].Kolor = GetFreeColor(KolorySasiadow);
                KolorySasiadow = 0;
            }
        }

        void GrafNaPlan()
        {
            uint ColorVal = 0;
            for (int color = 0; color < ColorCount; color++)
            {
                ColorVal = (uint)Mathf.Pow(2, color);
                for (int w = 0; w < Graf.Count; w++)
                {
                    W w1 = Graf[w];
                    if (w1.Kolor == ColorVal)
                    {
                        // sprawdzenie czy przedmiot nie przepelnia potoku 
                        if (GrupyPrzedmioty[w1.Grupa, w1.Przedmiot] > 0)
                        {
                            //PrintW(w1);
                            //Tmp.Add(w1);
                        }
                        else
                        {
                            // Usuwamy 
                            Graf.RemoveAt(w);
                            // Resetujemy kolor
                            for (int a = 0; a < Graf.Count; a++)
                            {
                                Graf[a].Kolor = 0;
                            }

                            return;
                        }

                    }
                }
                // Mozna dodac kolor
                for (int w = 0; w < Graf.Count; w++)
                {
                    W w1 = Graf[w];
                    if (w1.Kolor == ColorVal)
                    {
                        GrupyPrzedmioty[w1.Grupa, w1.Przedmiot]--;
                        w1.Godzina = Godzina;
                        Zajecia.Add(w1);
                        Graf.RemoveAt(w);
                    }
                }
                Godzina++;
            }
        }
    }
    void PrintW(W w)
    {
        Debug.Log("Godzina: " + w.Godzina + " " 
            + Grupy[w.Grupa] + " "
            + Przedmioty[w.Przedmiot] + " "
            + Nauczyciele[w.Nauczyciel] + " "
            + Sale[w.Sala]);
    }

    int ColorCount;
    uint GetFreeColor(uint color)
    {
        for (int a = 0; a < 32; a++)
        {
            if ((color & 1) == 0)
            {
                ColorCount = Mathf.Max(ColorCount, a);
                return (uint)Mathf.Pow(2, a);
            }
            color = color >> 1;
        }
        
        Debug.Log("Pzrekroczono ilosc kolorow");
        return 0;
    }

    uint SumColor(uint a, uint b)
    {
        if ((a & b) > 0)
        {
            // Kolor juz wystepuje
            return a;
        } else
        {
            return a + b;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
