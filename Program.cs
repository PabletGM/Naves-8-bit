//Pablo Garcia Moreno

using System;
using System.Threading;
using System.Diagnostics;

namespace naves
{
    class Program
    {
        const bool DEBUG = true;
        //const int ANCHO = 30, ALTO = 15;
        const int ANCHO = 30, ALTO = 15;
        static Random rnd = new Random();
        
        static void Main()
        {
            int[] suelo = new int[ANCHO];
            int[] techo=new int[ANCHO];//tanto suelo como techo de tamaño ANCHO
            int naveC = -1, naveF = -1, posInicialNave = -1,
                balaC = -1, balaF = -1,
                enemigoC = -1, enemigoF = -1;
            int colC = -1, colF = -1;
            bool juego = true;
            
            bool crashNave = false;
            
            

            #region  Estado Inicial del Juego
            #endregion 
            // inicializacion variables 
            RenderInicial(suelo, techo, ref naveC, ref naveF,ref posInicialNave);
             #region  Extras de musica
        #endregion
            //Ubicación del programa VLC cancion
        
            string player = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            //Ruta  DE LA CANCIÓN SELECCIONADA 
            string args1 = @"--intf dummy  C:\Users\pgmla\Desktop\TheWildSide.wav"; 
            //Ejecuta el sonido
            Process p = Process.Start(player, args1);
            #region  Bucle Principal
            #endregion
            //bucle principal
            while (juego)
            {   //Lee las teclas que pulsas
                char ch = LeeInput();
                //Si se pulsa q se termina el juego y la musica
                if (ch == 'q')
                { 
                    AcabarJuego(ref juego); 
                    p.Kill();
                }
                //Si se pulsa p se para el juego
                else if (ch == 'p') 
                {
                    PausarJuego();
                    p.Kill();
                }
                //Sino se pulsa p o q
                else
                {
                    //desplaza todas las columnas una posicion a la izquierda dando sensacion de movimiento
                    AvanzaTunel(suelo, techo);
                    //crea enemigo en limites tunel y se encarga de su logica
                    GeneraAvanzaEnemigo(ref enemigoC, ref enemigoF, suelo, techo);
                    //se encarga de la logica de la nave.
                    AvanzaNave( ch,ref naveC,ref naveF,  enemigoC,  enemigoF,techo,suelo);
                    //se encarga de la logica de la bala al pulsar x y crearlas 
                    GeneraAvanzaBala(ch, ref balaC,ref  balaF, naveC, naveF, enemigoC, enemigoF, suelo, techo);
                    //Se encarga de la colision de la nave
                    crashNave = ColisionNave(ref naveC,ref naveF, suelo, techo, enemigoC, enemigoF);
                    if (crashNave)
                    {
                        juego = false;
                        
                       
                    }
                    //colision de bala  
                    ColisionBala(ref balaC,ref balaF, ref enemigoC,ref enemigoF, suelo, techo,ref colC,ref colF);
                    //Renderiza tunel, de la nave y el enemigo , pero sus posiciones son calculadas antes
                    Render(suelo, techo, naveC, naveF, balaC,  balaF,ref enemigoC,ref enemigoF, colC, colF, ch, crashNave);
                    Thread.Sleep(120);
                }   
            }
            Console.SetCursorPosition(0, ALTO + 15);
            //Deja de reproducir el audio
            p.Kill();

            static void AvanzaTunel(int[] suelo, int[] techo)//avanza cada numero de cuadraditos azules en la posicion anterior, y aplica un movimiento al tunel de manera aleatoria
            {
                for (int i = 0; i < ANCHO - 1; i++)
                { // desplazamiento de eltos a la izda , al moverse todos a la posicion de al lado la primera posicion se queda vacia, ya que si techo[0]=2 y techo[1]=3, ahora techo[1]=2 reconvirtiendolo graficamente sin desplazarlo para dar la sensacion de movimiento, asi las ultimas posiciones no tienen referencia para cambiar
                    techo[i] = techo[i + 1];//Avanzas a siguiente posicion del array , de 1 en 1
                    suelo[i] = suelo[i + 1];
                }

                int s, t; // ultimas posiciones de suelo y techo que ya no existen se guardan
                s = suelo[ANCHO - 1]; //asocias s a el ultimo valor del suelo que está en la posicion ANCHO-1.
                t = techo[ANCHO - 1]; //asocias t a el ultimo valor del techo que está en la posicion ANCHO-1.

                // generamos nuevos valores para esas ultimas posiciones
                int opt = rnd.Next(5); // 5 alternativas
                if (opt == 0 && s < ALTO - 1) { s++; t++; }   // tunel baja    ,s < ALTO - 1 para comprobar si te queda suelo  ,s++ dice que el valor de s o suelo en ANCHO-1 va a bajar porque en el bucle le quedará menos para llegar a limite ALTO, con t++ el limite techo[i] vera aumentado su valor en 1 así hará 1 pintada mas de azul    
                else if (opt == 1 && t > 0) { s--; t--; }   // sube , al contrario que arriba , t > 0 mientras haya techo
                else if (opt == 2 && s - t > 7) { s--; t++; } // estrecha , el limite del suelo será una posición mas arriba y el del tunel una posicion mas abajo, s - t > 7 para que sea jugable y tenga espacio de tunel el jugador
                else if (opt == 3)
                {                    // ensancha , mientras se pueda con limite superior y posterior 
                    if (s < ALTO - 1) s++;
                    if (t > 0) t--;
                }
                // con 4 se deja igual, no se hace nada

                // asignamos ultimas posiciones en el array , estos valores modificados por el opt random ahora serán la nueva s y t
                suelo[ANCHO - 1] = s;
                techo[ANCHO - 1] = t;
            }
            static char LeeInput()
            {
                char ch = ' ';
                if (Console.KeyAvailable)
                {
                    string dir = Console.ReadKey(true).Key.ToString();
                    if (dir == "A" || dir == "LeftArrow") ch = 'l';//l=left
                    else if (dir == "D" || dir == "RightArrow") ch = 'r';//r=right
                    else if (dir == "W" || dir == "UpArrow") ch = 'u';//u=up
                    else if (dir == "S" || dir == "DownArrow") ch = 'd';//d=down
                    else if (dir == "X" || dir == "Spacebar") ch = 'x'; // bomba lanzar                  
                    else if (dir == "P") ch = 'p'; // pausa					
                    else if (dir == "Q" || dir == "Escape") ch = 'q'; // salir
                    while (Console.KeyAvailable) Console.ReadKey().Key.ToString();
                }
                return ch;
            }
            static void Render(int[] suelo, int[] techo, int naveC, int naveF,int balaC,  int balaF,ref int enemigoC,ref int enemigoF, int colC, int colF, char ch, bool crashNave)
            { 
                //Debug para saber posiciones de todo en cualquier momento
                if(DEBUG)
                {
                    Debug(suelo, techo, enemigoC,  enemigoF, naveC, naveF,balaC, balaF,colC,colF);
                }
                // renderizado de tunel ...
                RenderTunel(suelo, techo);
                // renderizado de enemigo ...
                DibujaEnemigo(  enemigoC, enemigoF);
                //renderizado de nave con o sin colision
                DibujaNave(naveC, naveF,crashNave);
                //renderizado bala con o sin colision
                DibujarBala( balaC,  balaF);
            }
            static void RenderInicial(int[] suelo, int[] techo,ref int naveC,ref int naveF,ref int posInicialNave)
            {
                posInicialNave = 0;
                //Para esconder cursor
                Console.CursorVisible = false;

                //Con un tunel inicial creado se crean nuevas columnas empezando por la ultima
                IniciaTunel(suelo, techo);
                // renderizado de tunel inicial aleatorio
                RenderTunel(suelo, techo);
                // renderizado de enemigo posicionInicial no , ya que se crea en GeneraAvanzaEnemigo

                //renderizado de nave posicion inicial
                posInicialNave= rnd.Next(techo[ANCHO/2] + 2, suelo[ANCHO/2] - 2);
                if (naveC == -1 && naveF == -1)
                {
                    //para que esté en una posicion par
                    naveC = posInicialNave;
                    naveF = (suelo[ANCHO / 2] - techo[ANCHO / 2]) / 2;//si resto suelo[ANCHO / 2]-techo[ANCHO / 2] me quedará la cantidad de bloques de espacio jugable a eso le dividimos entre 2
                }
                //renderizado de bala posicion inicial no se crea hasta que se pulse x



            }
            static void RenderTunel(int[] suelo, int[] techo)//guarda un valor aleatorio en cada columna para techo y suelo y lo renderiza
            {
               

                //Para recorrer columnas
                for (int i = 0; i < ANCHO; i++)
                {

                    //Relleno de array techo 
                    for (int j = 0; j <= techo[i]; j++)
                    {
                        Console.SetCursorPosition(2 * i, j);
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.WriteLine("  ");
                    }

                    Console.ResetColor();

                    //Relleno de array espacio de juego
                    for (int j = techo[i] + 1; j < suelo[i]; j++)
                    {
                        Console.SetCursorPosition(2 * i, j);
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.Write("  ");
                    }

                    //Relleno de array suelo
                    for (int j = suelo[i]; j < ALTO ; j++)
                    {
                        Console.SetCursorPosition(2 * i, j);
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.WriteLine("  ");
                    }
                    Console.ResetColor();

                }

            }
            static void IniciaTunel(int[] suelo, int[] techo)
            {
                techo[ANCHO - 1] = 0;
                suelo[ANCHO - 1] = ALTO - 1;
                for (int i = 1; i < ANCHO; i++)
                {
                    AvanzaTunel(suelo, techo);
                }
            }
            static void Debug(int[] suelo, int[] techo,  int enemigoC,  int enemigoF, int naveC, int naveF, int balaC, int balaF,int colC,int colF)
            {
                //Relleno de posiciones techo 
                for (int j = 0; j < ANCHO; j++)
                {
                    Console.SetCursorPosition(2 * j, ALTO + 2);
                    Console.WriteLine(techo[j]);
                }
                Console.SetCursorPosition(0, ALTO + 1);
                Console.Write("techo:");

                //Relleno de posiciones suelo
                for (int j = 0; j < ANCHO; j++)
                {
                    if (j == 0)
                    {
                        Console.SetCursorPosition(0, ALTO + 5);
                        Console.Write(suelo[j]);
                    }
                    else
                    {
                        Console.Write(" ");
                        if (suelo[j] < 10)
                        {
                            Console.Write(" ");
                            Console.Write(suelo[j]);
                        }
                        else
                        {
                            Console.Write(suelo[j]);
                        }
                    }
                }
                Console.SetCursorPosition(0, ALTO + 4);
                Console.Write("suelo:");
                //Posicion enemigoC y enemigoF
                Console.SetCursorPosition(0, ALTO + 7);
                Console.Write("enemigo: " + enemigoC + "," + enemigoF+" " );
                //Posicion naveC y naveF
                Console.SetCursorPosition(0, ALTO + 9);
                Console.Write("nave: " + naveC + "," + naveF);
                //Posicion de la bala
                Console.SetCursorPosition(0, ALTO + 11);
                Console.Write("bala: " + balaC + "," + balaF+" ");
                //Posicion de la colision
                Console.SetCursorPosition(0, ALTO + 13);
                Console.Write("colision: "  + colC + ","+colF+" ");
            }
            static void GeneraAvanzaEnemigo(ref int enemigoC, ref int enemigoF, int[] suelo, int[] techo)
            {
                //si no hay enemigo en juego
                if (enemigoC == -1 )
                {
                    // un cuarto de posibilidad de variable=0;
                    int GenerarEnemigo = rnd.Next(0, 4);

                    if ( GenerarEnemigo == 0)
                    {
                        //numero aleatorio entre limites de tunel, me puede coincidir con limite techo o suelo , el +2 y -2 es por pura estética y mejor jugabilidad
                        int filaEspacioJuego = rnd.Next(techo[ANCHO - 1]+2, suelo[ANCHO - 1]-2);
                        //ponemos el valor donde aparecerán el enemigoC y enemigoF para dejar claro que ya existen
                        enemigoC = (ANCHO - 1);
                        enemigoF = filaEspacioJuego;
                    }
                } 
                //si hay enemigo en juego
                else if(enemigoC != -1 && enemigoF != -1)
                {
                    //mover una posicion a la izq
                    enemigoC = enemigoC - 1;
                }               
            }
            static void DibujaEnemigo(  int enemigoC,  int enemigoF)
            {
                //para primer ciclo si existen tendrán valor 1 desde GeneraAvanzaEnemigos
                if (enemigoC != -1)
                {
                    //creamos enemigo en ultima columna
                    Console.SetCursorPosition(enemigoC*2, enemigoF);//enemigoC*2 ATENTO
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("<>");
                    Console.ResetColor();
                }                   
            }
            static void AvanzaNave( char ch,ref int naveC,ref int naveF, int enemigoC, int enemigoF,int [] techo,int [] suelo)
            {
                    //si no coincide posicion de enemigo , se puede aplicar el movimiento del input
                    if(naveC!=enemigoC && naveF==enemigoF|| naveC != enemigoC  && naveF != enemigoF|| naveC == enemigoC  && naveF != enemigoF )
                    {
                        //izquierda
                        if (ch == 'l')
                        {
                            if (naveC > 0)
                            {
                                naveC -= 1;
                            }
                        }
                        //derecha
                        else if (ch == 'r')
                        {//si está en limites de juego 
                            if (naveC < (ANCHO - 1) )
                            {
                                
                                naveC += 1;
                            }
                        }
                        //arriba
                        else if (ch == 'u')
                        {//si está en limites de juego
                            if (naveF > 0)
                            {
                                naveF -= 1;
                            }
                        }
                        //abajo
                        else if (ch == 'd')
                        {//si está en limites de juego
                            if (naveF < ALTO - 1)
                            {
                                naveF += 1;
                            }
                        }
                    }  
            }
            static void DibujaNave( int naveC, int naveF,bool crashNave)
            {
                int NaveC=0;
                int NaveF=0;
                if (crashNave)
                {
                    //almacenas posicion de colision en 2 nuevas variables
                     NaveC = naveC;
                     NaveF = naveF;
                    //la nave deja de existir
                    naveC = -1;
                    naveF = -1;
                }
                //si existe dibujamos la nave
                if(naveC!=-1&&naveF!=-1)
                {
                    
                    Console.SetCursorPosition(naveC * 2, naveF);
                    Console.ForegroundColor = ConsoleColor.Green; ;
                    Console.Write("=>");
                } 
                //si la nave ya no existe , dibujamos su colision
                else
                {
                    Console.SetCursorPosition(NaveC * 2, NaveF);
                    Console.ForegroundColor = ConsoleColor.Red; ;
                    Console.Write("=*");

                }
                Console.ResetColor();
            }
            static void GeneraAvanzaBala( char ch ,ref int balaC,ref int balaF, int naveC, int naveF,int enemigoC,int enemigoF,int[] suelo,int[]techo)
            {
                //sino hay bala en juego se crea
                if (ch == 'x')
                {
                    //si no existia se le añade posicion inicial a la bala
                    if (balaC == -1 && balaF == -1)
                    {
                         balaC = naveC + 1;
                        balaF = naveF;
                    }                
                }
                //si ya existe la bala y no está ni sobre el tunel(suelo o techo) ni sobre el enemigo, ni sobrepasa el limite avanza hacia la derecha
                else if(balaC != -1 && balaF != -1)
                {
                    if (balaC != enemigoC  && balaF > techo[balaC] && balaF < suelo[balaC]&&balaC<ANCHO)
                    {
                        //se mueve una posicion a la derecha
                        balaC += 1;
                    }
                }    
            }
            static void DibujarBala(int balaC,int balaF)
            {

                //renderizado bala mientras exista
                if (balaC>0&&balaF>0)
                {   
                        Console.SetCursorPosition(balaC*2, balaF);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("--");
                        Console.ResetColor();    
                }
            }
            static void AcabarJuego(ref bool juego)
            {
                //Termina la partida
                Console.SetCursorPosition(0, ALTO + 11);
                juego = false;
            }
            static void PausarJuego()
            {
                //pausamos pantalla mientras escribimos en un lateral del juego Pause
                Console.SetCursorPosition(ANCHO-4, ALTO / 2);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("P  A  U  S  E");
                //PULSA CUALQUIER TECLA PARA CONTINUAR
                Console.SetCursorPosition(ANCHO-10, (ALTO / 2)+2);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Pulsa cualquier tecla para continuar");
                //paramos juego
                Console.ReadKey();
                Console.ResetColor();
            }
            static bool ColisionNave(ref int naveC,ref int naveF, int[] suelo, int[] techo, int enemigoC, int enemigoF)
            {//colisione con suelo , techo o enemigo
                bool NaveColision = false;
                //si la fila en la que se encuentra la nave >=valor de el suelo   en posicion  o columnanaveC , significa que está fuera de los limites del suelo , o limites de techo o coincidir con enemigo
                if (naveC>0&&naveF >= suelo[naveC]|| naveC > 0 && naveF <= techo[naveC]|| enemigoC == naveC && enemigoF == naveF|| naveF <= techo[naveC+1]&& naveC > 0)
                {
                    NaveColision = true;
                }
                
                return NaveColision;
            }
            static void ColisionBala(ref int balaC,ref int balaF,ref int enemigoC,ref int enemigoF,int[]suelo,int []techo,ref int colC, ref int colF)
            {
                //si existe
                if(balaC!=-1&&balaF!=-1)
                {
                    //bala se destruye al llegar al final de la pista
                    if (balaC >= ANCHO )
                    {
                        balaC = -1;
                        balaF = -1;
                    }
                    //si coincide posicion bala con enemigo
                    else if (balaC == enemigoC && balaF == enemigoF|| balaC+1 == enemigoC && balaF == enemigoF)
                    {
                        //asignamos posicion de colision
                        colC = balaC;
                        colF = balaF;
                        //eliminamos bala
                        balaC = -1;
                        balaF = -1;
                        //eliminamos enemigo
                        enemigoC = -1;
                        enemigoF = -1;  
                    }
                    //colision bala con techo y suelo

                    else if(balaF<=techo[balaC])
                    {
                        colC = balaC;
                        colF = balaF;
                        //eliminamos esa posicion del techo del tunel restandole 1 al array
                        for(int i=techo[balaC];i>=balaF;i--)
                        {
                                techo[balaC] -= 1;
                        }
                        //eliminamos bala
                        balaC = -1;
                        balaF = -1;
                    }
                    else if(balaF>=suelo[balaC])
                    {
                        colC = balaC;
                        colF = balaF;              
                        //eliminamos esa posicion del techo del tunel restandole 1 al array
                        for(int i=suelo[balaC];i<=balaF;i++)
                        {
                           suelo[balaC] += 1;
                        }
                        //eliminamos bala
                        balaC = -1;
                        balaF = -1;
                    }
                    else
                    {
                        colC = -1;
                        colF = -1;
                    }   
                }   
            }
        }
    }
}
